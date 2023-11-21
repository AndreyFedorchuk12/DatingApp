using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class RegisterDto
{
    [Required]
    public required string UserName { get; set; }

    [Required]
    [StringLength(15, MinimumLength = 4)]
    public required string Password { get; set; }
}