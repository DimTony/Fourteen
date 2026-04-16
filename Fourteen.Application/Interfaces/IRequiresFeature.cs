using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fourteen.Application.Interfaces
{
    public interface IRequiresFeature
    {
        string FeatureFlag { get; }
    }
}
