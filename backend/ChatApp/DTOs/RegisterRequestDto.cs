using System.ComponentModel.DataAnnotations;

namespace ChatApp.DTOs
{
    
    public sealed class RegisterRequestDto
    {
        [Required] public string Username { get; init; } = string.Empty;
        [Required, EmailAddress] public string Email { get; init; } = string.Empty;
        [Required] public string Password { get; init; } = string.Empty;
    }
}