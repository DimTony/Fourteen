using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fourteen.Domain.Common
{
    public record ProfileId(Guid Value)
    {
        public static ProfileId New() => new(Guid.CreateVersion7());
        public override string ToString() => Value.ToString();
    }

    public record UserId(Guid Value)
    {
        public static UserId New() => new(Guid.CreateVersion7());
        public override string ToString() => Value.ToString();
    }

    public record RefreshTokenId(Guid Value)
    {
        public static RefreshTokenId New() => new(Guid.CreateVersion7());
        public override string ToString() => Value.ToString();
    }
}
