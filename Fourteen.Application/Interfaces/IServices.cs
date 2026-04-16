using Fourteen.Application.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fourteen.Application.Interfaces
{
    public interface IServices
    {
        Task<ExternalAPIDto> GetByName(string name, CancellationToken ct);

        Task<TransformedExternalResponse> FetchAll(string name, CancellationToken ct);
    }
}
