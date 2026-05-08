using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Features.Profiles.Queries.GetProfiles;
using System.Text;

namespace Fourteen.Application.Common.Helpers
{

    public static class CacheKeyBuilder
    {
        private const string Prefix = "profiles";
        private const string SearchPrefix = "search";

        public static string ForGetProfiles(GetProfilesQuery q)
        {
            var parts = new SortedDictionary<string, string>(StringComparer.Ordinal);

            AddIfPresent(parts, "gender",      q.Gender);
            AddIfPresent(parts, "age_group",   q.AgeGroup);
            AddIfPresent(parts, "country_id",  q.CountryId?.ToUpperInvariant()); // ISO codes are uppercase
            AddIntRange (parts, "age",         q.MinAge,   q.MaxAge);
            AddFloatRange(parts, "g_prob",     q.MinGenderProbability,  q.MaxGenderProbability);
            AddFloatMin (parts, "c_prob",      q.MinCountryProbability);
            AddIfPresent(parts, "sort",        $"{Normalise(q.SortBy)}.{Normalise(q.Order)}");

            var filterSegment = BuildSegment(parts);
            return $"{Prefix}:{filterSegment}|p={q.Page}&l={q.Limit}";
        }

        public static string ForSearch(ParsedProfileFilter f, int page, int limit)
        {
            var canonical = Canonicalise(f);
            return $"{SearchPrefix}:{canonical}|p={page}&l={limit}";
        }

        public static string Canonicalise(ParsedProfileFilter f)
        {
            var parts = new SortedDictionary<string, string>(StringComparer.Ordinal);

            AddIfPresent(parts, "gender",     f.Gender);
            AddIfPresent(parts, "country_id", f.CountryId?.ToUpperInvariant());
            AddIntRange (parts, "age",        f.AgeMin, f.AgeMax);

            return BuildSegment(parts);
        }

        public static string ForProfileById(Guid id) => $"profile:{id}";

        private static void AddIfPresent(SortedDictionary<string, string> d, string key, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                d[key] = Normalise(value);
        }

        private static void AddIntRange(SortedDictionary<string, string> d, string key, int? min, int? max)
        {
            if (min is null && max is null) return;
            d[key] = $"{(min.HasValue ? min.Value.ToString() : "*")}-{(max.HasValue ? max.Value.ToString() : "*")}";
        }

        private static void AddFloatRange(SortedDictionary<string, string> d, string key, float? min, float? max)
        {
            if (min is null && max is null) return;
            var lo = min.HasValue ? $"{min.Value:F2}" : "*";
            var hi = max.HasValue ? $"{max.Value:F2}" : "*";
            d[key] = $"{lo}-{hi}";
        }

        private static void AddFloatMin(SortedDictionary<string, string> d, string key, float? min)
        {
            if (min is null) return;
            d[key] = $"{min.Value:F2}-*";
        }

        private static string BuildSegment(SortedDictionary<string, string> parts)
        {
            if (parts.Count == 0) return "all";

            var sb = new StringBuilder();
            foreach (var (k, v) in parts)
            {
                if (sb.Length > 0) sb.Append('&');
                sb.Append(k).Append('=').Append(v);
            }

            return sb.ToString();
        }

        private static string Normalise(string? s) =>
            string.IsNullOrWhiteSpace(s) ? string.Empty : s.Trim().ToLowerInvariant();
    }
}