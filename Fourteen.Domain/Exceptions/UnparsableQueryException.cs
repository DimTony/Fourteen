using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fourteen.Domain.Exceptions
{
    public sealed class UnparsableQueryException : DomainException
    {
        public string RawQuery { get; }

        public UnparsableQueryException(string rawQuery)
            : base($"Could not extract any filters from query: '{rawQuery}'")
        {
            RawQuery = rawQuery;
        }
    }
}
