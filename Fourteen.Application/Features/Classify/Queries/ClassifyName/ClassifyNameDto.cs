using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fourteen.Application.Features.Classify.Queries.ClassifyName
{
    public sealed record ClassifyNameDto(
        string Name,
        string Gender,
        double Probability,
        int SampleSize,
        bool IsConfident,
        string ProcessedAt);
}
