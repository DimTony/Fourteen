using Fourteen.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fourteen.Domain.Aggregates.Profiles
{
    public class Profile : AggregateRoot<ProfileId>
    {
        public string Name { get; private set; } = default!;
        public string Gender { get; private set; } = default!;
        public double GenderProbability { get; private set; } = default!;
        // public int SampleSize { get; private set; } = default!;
        public int Age { get; private set; } = default!;
        public string AgeGroup { get; private set; } = default!;
        public string CountryId { get; private set; } = default!;
        public string CountryName { get; private set; } = default!;
        public double CountryProbability { get; private set; } = default!;
        public DateTime CreatedAt { get; private set; }

        private Profile() { }

        public static Profile Create(
            string name,
            string gender,
            double genderProbability,
            // int sampleSize,
            int age,
            string ageGroup,
            string countryId,
            string countryName,
            double countryProbability)
        {
            return new Profile
            {
                Id = ProfileId.New(),
                Name = name,
                Gender = gender,
                GenderProbability = genderProbability,
                // SampleSize = sampleSize,
                Age = age,
                AgeGroup = ageGroup ?? AgeGroupClassifier.Classify(age),
                CountryId = countryId,
                CountryName = countryName,
                CountryProbability = countryProbability,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static class AgeGroupClassifier
        {
            public static string Classify(int age)
            {
                if (age <= 12) return "child";
                if (age <= 19) return "teenager";
                if (age <= 59) return "adult";
                return "senior";
            }
        }
    }

}
