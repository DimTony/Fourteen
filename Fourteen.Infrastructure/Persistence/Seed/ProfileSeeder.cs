using System.Text.Json;
using Fourteen.Domain.Aggregates.Profiles;
using Fourteen.Application.Common.DTOs;

namespace Fourteen.Infrastructure.Persistence.Seed
{
    public class ProfileSeeder(AppDbContext context)
    {
        public async Task SeedAsync(string jsonPath)
        {
            var json = await File.ReadAllTextAsync(jsonPath);
            var file = JsonSerializer.Deserialize<SeedFile>(json)!;
            var raw = file.Profiles;

            var existingNames = context.Profiles
                .Select(p => p.Name)
                .ToHashSet();

            var toInsert = raw
                .Where(r => !existingNames.Contains(r.Name))
                .Select(r => Profile.Create(
                    r.Name, r.Gender, r.GenderProbability, r.SampleSize,
                    r.Age, r.AgeGroup, r.CountryId, r.CountryName, r.CountryProbability))
                .ToList();

            if (toInsert.Any())
            {
                await context.Profiles.AddRangeAsync(toInsert);
                await context.SaveChangesAsync();
                Console.WriteLine($"Seeded {toInsert.Count} profiles.");
            }
            else
            {
                Console.WriteLine("No new profiles to seed.");
            }
        }
        public async Task OldSeedAsync(string jsonPath)
        {
            var json = await File.ReadAllTextAsync(jsonPath);
            var raw = JsonSerializer.Deserialize<SeedFile>(json)!;

            var existingNames = context.Profiles
                .Select(p => p.Name)
                .ToHashSet();

            var toInsert = raw.Profiles
                .Where(r => !existingNames.Contains(r.Name))
                .Select(r => Profile.Create(
                    r.Name, r.Gender, r.GenderProbability, r.SampleSize,
                    r.Age, r.AgeGroup, r.CountryId, r.CountryName, r.CountryProbability))
                .ToList();

            if (toInsert.Any())
            {
                await context.Profiles.AddRangeAsync(toInsert);
                await context.SaveChangesAsync();
            }
        }
    }
}