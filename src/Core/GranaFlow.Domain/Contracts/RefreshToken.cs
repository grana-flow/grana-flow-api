using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GranaFlow.Domain.Contracts;

public record class RefreshToken
{
    [Required(ErrorMessage = "{0} is required")]
    [JsonPropertyName("refresh_token")]
    public required string Refresh { get; set; }

    [Required(ErrorMessage = "{0} is required")]
    [JsonPropertyName("email")]
    public required string Email { get; set; }
}
