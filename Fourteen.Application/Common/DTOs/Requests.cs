using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Fourteen.Application.Common.DTOs
{
    public class CreateProfileRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;
    }
}
