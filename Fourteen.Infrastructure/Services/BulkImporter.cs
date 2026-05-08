using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;
using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Aggregates.Profiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fourteen.Infrastructure.Services
{
    public sealed class BulkProfileImporter : IBulkProfileImporter
    {
        private const int ChunkSize = 500;

        private static readonly HashSet<string> ValidGenders =
            new(StringComparer.OrdinalIgnoreCase) { "male", "female" };

        private readonly IDbContextFactory<Persistence.AppDbContext> _dbFactory;
        private readonly ILogger<BulkProfileImporter> _logger;

        public BulkProfileImporter(
            IDbContextFactory<Persistence.AppDbContext> dbFactory,
            ILogger<BulkProfileImporter> logger)
        {
            _dbFactory = dbFactory;
            _logger = logger;
        }

        public async Task<BulkImportResult> Import(
            Stream csvStream,
            CancellationToken ct = default)
        {
            await using var ctx = await _dbFactory.CreateDbContextAsync(ct);
            var existingNames = await ctx.Profiles.AsNoTracking()
                .Select(p => p.Name).ToHashSetAsync(StringComparer.OrdinalIgnoreCase, ct);

            var counters = new ImportCounters();
            var chunk = new List<Profile>(ChunkSize);

            _logger.LogInformation("BulkImport: loaded {Count} existing names", existingNames.Count);

            using var reader = new StreamReader(csvStream, Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true,
                bufferSize: 65536,
                leaveOpen: true);

            var firstLine = await reader.ReadLineAsync(ct);
            if (firstLine is not null)
            {
                var cols = SplitCsvLine(firstLine);
                if (!IsHeaderRow(cols))
                    ProcessRow(cols, existingNames, chunk, counters);
            }

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync(ct);
                if (line is null) break;

                var cols = SplitCsvLine(line);
                ProcessRow(cols, existingNames, chunk, counters);

                if (chunk.Count >= ChunkSize)
                await FlushChunkAsync(ctx, chunk, counters, ct);
            }

            if (chunk.Count > 0)
            await FlushChunkAsync(ctx, chunk, counters, ct);

            _logger.LogInformation(
                "BulkImport complete: total={Total} inserted={Inserted} skipped={Skipped}",
                counters.TotalRows, counters.Inserted, counters.Skipped);

             return new BulkImportResult
    {
        TotalRows = counters.TotalRows,
        Inserted  = counters.Inserted,
        Skipped   = counters.Skipped,
        Reasons   = counters.Reasons
    };
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessRow(string[] cols, HashSet<string> existingNames,
            List<Profile> chunk, ImportCounters counters)
        {
            counters.TotalRows++;

            if (cols.Length < CsvColumns.RequiredCount)
            {
                counters.Reasons.MalformedRow++;
                counters.Skipped++;
                return;
            }

            var name = cols[CsvColumns.Name].Trim();
            var gender = cols[CsvColumns.Gender].Trim();
            var ageStr = cols[CsvColumns.Age].Trim();
            var countryId = cols[CsvColumns.CountryId].Trim();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(gender) ||
                string.IsNullOrEmpty(ageStr) || string.IsNullOrEmpty(countryId))
            {
                counters.Reasons.MissingFields++;
                counters.Skipped++;
                return;
            }

            if (!ValidGenders.Contains(gender))
            {
                counters.Reasons.InvalidGender++;
                counters.Skipped++;
                return;
            }

            if (!int.TryParse(ageStr, out var age) || age < 0 || age > 150)
            {
                counters.Reasons.InvalidAge++;
                counters.Skipped++;
                return;
            }

            var nameLower = name.ToLowerInvariant();
            if (!existingNames.Add(nameLower))
            {
                counters.Reasons.DuplicateName++;
                counters.Skipped++;
                return;
            }

            _ = double.TryParse(cols[CsvColumns.GenderProbability].Trim(),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out var genderProb);

            var ageGroup = cols[CsvColumns.AgeGroup].Trim();
            var countryName = cols[CsvColumns.CountryName].Trim();

            _ = double.TryParse(cols[CsvColumns.CountryProbability].Trim(),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out var countryProb);

            try
            {
                var profile = Profile.Create(
                    name: nameLower,
                    gender: gender.ToLowerInvariant(),
                    genderProbability: genderProb,
                    sampleSize: 0,
                    age: age,
                    ageGroup: string.IsNullOrEmpty(ageGroup) ? null : ageGroup,
                    countryId: countryId.ToUpperInvariant(),
                    countryName: string.IsNullOrEmpty(countryName) ? null : countryName,
                    countryProbability: countryProb);

                chunk.Add(profile);
            }
            catch
            {
                counters.Reasons.MalformedRow++;
                counters.Skipped++;
                existingNames.Remove(nameLower);
            }
        }


        private async Task FlushChunkAsync(
            Persistence.AppDbContext ctx,
            List<Profile> chunk,
            ImportCounters counters,
            CancellationToken ct)
        {
            if (chunk.Count == 0) return;

            try
            {
                await ctx.Profiles.AddRangeAsync(chunk, ct);
                await ctx.SaveChangesAsync(ct);

                counters.Inserted += chunk.Count;

                _logger.LogDebug(
                    "BulkImport: flushed chunk of {Count}",
                    chunk.Count);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogWarning(
                    ex,
                    "BulkImport: chunk of {Count} failed to persist, skipping",
                    chunk.Count);
            }
            finally
            {
                chunk.Clear();
                ctx.ChangeTracker.Clear();
            }
        }
        private static string[] SplitCsvLine(string line)
        {
            var result = new List<string>(CsvColumns.RequiredCount + 2);
            var sb = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(sb.ToString());
                    sb.Clear();
                }
                else
                {
                    sb.Append(c);
                }
            }

            result.Add(sb.ToString());
            return result.ToArray();
        }
        private static bool IsHeaderRow(string[] cols)
        {
            if (cols.Length == 0) return false;
            var first = cols[0].Trim().ToLowerInvariant();
            return first is "name" or "profile_name" or "full_name";
        }


    }
}