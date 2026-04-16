using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fourteen.Domain.Exceptions
{
    public sealed class NoPredictionException : DomainException
    {
        public NoPredictionException(string message) : base(message) { }
    }
}
