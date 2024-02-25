using System.Text.Json.Serialization;

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
