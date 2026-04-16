using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fourteen.Domain.Exceptions
{
    public sealed class InvalidNameException : DomainException
    {
        public InvalidNameException(string message) : base(message) { }
    }
}
