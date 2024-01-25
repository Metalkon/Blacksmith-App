using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Shared_Classes.Models
{
    public class TokenDTO
    {
        [JsonPropertyName("refreshToken")]
        public string? RefreshToken { get; set; }
        [JsonPropertyName("tokenExp")]
        public string? Jwt { get; set; }
    }
}
