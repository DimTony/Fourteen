using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fourteen.Domain.Exceptions
{
    public sealed class UpstreamApiException : DomainException
    {
        public int? StatusCode { get; }
        public UpstreamApiException(string message, int? statusCode = null)
            : base(message) => StatusCode = statusCode;
    }
}
