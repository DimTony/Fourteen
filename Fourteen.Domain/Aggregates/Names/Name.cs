using Fourteen.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fourteen.Domain.Aggregates.Names
{
    public sealed record Name
    {
        public string Value { get; }

        private Name(string value) => Value = value;

        public static Name Create(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                throw new InvalidNameException("Missing or empty name parameter");

            var trimmed = raw.Trim();

            if (trimmed.Length > 200)
                throw new InvalidNameException("Name exceeds maximum allowed length");

            if (double.TryParse(trimmed, out _))
                throw new InvalidNameException("Name must be a valid string, not a number");

            return new Name(trimmed);
        }
    }
}
