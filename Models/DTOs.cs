using System.ComponentModel.DataAnnotations;
using dotnet_auth.Validation;

namespace dotnet_auth.Models;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [Password]
    public string Password { get; set; }

    [NotEmpty]
    [StringLength(50)]
    public string FirstName { get; set; }

    [NotEmpty]
    [StringLength(50)]
    public string LastName { get; set; }
}

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }
}

public class ProductDto
{
    [NotEmpty]
    [StringLength(100)]
    public string Name { get; set; }

    [StringLength(500)]
    public string Description { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [NotEmpty]
    [StringLength(50)]
    public string Category { get; set; }
}

public class ResetPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Token { get; set; }

    [Required]
    [Password]
    public string Password { get; set; }
}

public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}