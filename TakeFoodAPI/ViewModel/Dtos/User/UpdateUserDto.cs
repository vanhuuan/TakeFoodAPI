using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TakeFoodAPI.ViewModel.Dtos.User;

public class UpdateUserDto
{
    /// <summary>
    /// Display Name
    /// </summary>
    [JsonPropertyName("name")]
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// Email
    /// </summary>
    [JsonPropertyName("email")]
    [Required]
    public string Email { get; set; }

    /// <summary>
    /// Số điện thoại
    /// </summary>
    [JsonPropertyName("phoneNumber")]
    [Required]
    public string PhoneNumber { get; set; }
}
