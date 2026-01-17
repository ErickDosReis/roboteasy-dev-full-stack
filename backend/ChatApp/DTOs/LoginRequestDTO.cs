using System.ComponentModel.DataAnnotations;

namespace ChatApp.DTOs
{
    public sealed class LoginRequestDto
    {
        [Required] public string Username { get; init; } = string.Empty;
        [Required] public string Password { get; init; } = string.Empty;
    }
}