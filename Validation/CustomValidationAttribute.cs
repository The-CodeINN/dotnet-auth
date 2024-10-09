using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace dotnet_auth.Validation;

public class PasswordAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var password = value as string;

        if (string.IsNullOrWhiteSpace(password))
            return new ValidationResult("Password is required.");

        var hasNumber = new Regex(@"[0-9]+");
        var hasUpperChar = new Regex(@"[A-Z]+");
        var hasLowerChar = new Regex(@"[a-z]+");
        var hasSymbols = new Regex(@"[!@#$%^&*(),.?"":{}|<>]+");
        var hasMinLength = new Regex(@".{8,}");

        if (!hasNumber.IsMatch(password))
            return new ValidationResult("Password must contain at least one number.");

        if (!hasUpperChar.IsMatch(password))
            return new ValidationResult("Password must contain at least one upper case letter.");

        if (!hasLowerChar.IsMatch(password))
            return new ValidationResult("Password must contain at least one lower case letter.");

        if (!hasSymbols.IsMatch(password))
            return new ValidationResult("Password must contain at least one special character.");

        if (!hasMinLength.IsMatch(password))
            return new ValidationResult("Password must be at least 8 characters long.");

        return ValidationResult.Success;
    }
}

public class NotEmptyAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is string stringValue)
        {
            if (string.IsNullOrWhiteSpace(stringValue))
                return new ValidationResult($"{validationContext.DisplayName} cannot be empty.");
        }

        return ValidationResult.Success;
    }
}