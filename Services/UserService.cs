using System.Security.Cryptography;
using dotnet_auth.Models;
using dotnet_auth.Repository;
using BC = BCrypt.Net.BCrypt;
namespace dotnet_auth.Services;

public interface IUserService
{
    Task<(bool Success, string Message)> RegisterUserAsync(User user, string password);
    Task<(bool Success, string Message, string AccessToken, string RefreshToken)> LoginUserAsync(string email, string password, string ipAddress);
    Task<(bool Success, string Message, string AccessToken, string RefreshToken)> RefreshTokenAsync(string refreshToken, string ipAddress);
    Task<(bool Success, string Message)> VerifyEmailAsync(string email, string token);
    Task<(bool Success, string Message)> ResetPasswordAsync(string email, string token, string newPassword);
    Task<(bool Success, string Message)> SendPasswordResetEmailAsync(string email);
    Task<(bool Success, string Message)> DeleteSessionAsync(string refreshToken, string ipAddress);
    Task<(bool Success, string Message)> RevokeAllUserTokensAsync(int userId, string ipAddress);
    Task<User> GetUserAsync(int userId);
}


public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly ITokenService _tokenService;

    public UserService(IUserRepository userRepository, IConfiguration configuration, IEmailService emailService, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _emailService = emailService;
        _tokenService = tokenService;
    }

    public async Task<(bool Success, string Message)> RegisterUserAsync(User user, string password)
    {
        var existingUser = await _userRepository.GetUserByEmailAsync(user.Email);
        if (existingUser != null)
        {
            return (false, "User already exists");
        }

        user.PasswordHash = BC.HashPassword(password);
        user.IsEmailConfirmed = false;
        user.EmailConfirmationToken = GenerateRandomToken();
        user.EmailConfirmationTokenExpires = DateTime.UtcNow.AddHours(24);

        var createdUser = await _userRepository.CreateUserAsync(user);

        // Send confirmation email
        var confirmationLink = $"{_configuration["AppUrl"]}/email/verify/{user.EmailConfirmationToken}";
        await _emailService.SendConfirmationEmailAsync(user, confirmationLink);

        // Send welcome email
        await _emailService.SendWelcomeEmailAsync(user);

        return (true, "User registered successfully");
    }
    public async Task<(bool Success, string Message, string AccessToken, string RefreshToken)> LoginUserAsync(string email, string password, string ipAddress)
    {
        var user = await _userRepository.GetUserByEmailAsync(email);

        if (user == null || !BC.Verify(password, user.PasswordHash))
        {
            return (false, "Invalid email or password", null, null);
        }

        if (!user.IsEmailConfirmed)
        {
            return (false, "Email not confirmed", null, null);
        }

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken(ipAddress);

        user.RefreshTokens.Add(refreshToken);
        await _userRepository.UpdateUserAsync(user);

        return (true, "Login successful", accessToken, refreshToken.Token);
    }

    public async Task<(bool Success, string Message, string AccessToken, string RefreshToken)> RefreshTokenAsync(string refreshToken, string ipAddress)
    {
        var token = await _userRepository.GetRefreshTokenAsync(refreshToken);

        if (token == null)
            return (false, "Invalid token", null, null);

        if (!token.IsActive)
            return (false, "Inactive token", null, null);

        // Replace old refresh token with a new one and save
        var newRefreshToken = _tokenService.GenerateRefreshToken(ipAddress);
        token.Revoked = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        token.ReplacedByToken = newRefreshToken.Token;
        token.User.RefreshTokens.Add(newRefreshToken);
        await _userRepository.UpdateUserAsync(token.User);

        // Generate new access token
        var accessToken = _tokenService.GenerateAccessToken(token.User);

        return (true, "Token refreshed", accessToken, newRefreshToken.Token);
    }

    public async Task<(bool Success, string Message)> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await _userRepository.GetUserByEmailAsync(email);

        if (user == null || user.PasswordResetToken != token || user.PasswordResetTokenExpires < DateTime.UtcNow)
        {
            return (false, "Invalid or expired reset token");
        }

        // Double-check that all tokens are revoked
        await RevokeAllUserTokensAsync(user.Id, "Password Reset");

        user.PasswordHash = BC.HashPassword(newPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpires = null;

        await _userRepository.UpdateUserAsync(user);

        // Notify user about password change
        await _emailService.SendPasswordResetSuccessEmailAsync(user);

        return (true, "Password reset successfully");
    }

    public async Task<(bool Success, string Message)> RevokeAllUserTokensAsync(int userId, string ipAddress)
    {
        try
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null) return (false, "User not found");

            // Revoke all active refresh tokens
            foreach (var token in user.RefreshTokens.Where(t => t.IsActive))
            {
                token.Revoked = DateTime.UtcNow;
                token.RevokedByIp = ipAddress;
                token.ReplacedByToken = null;
            }

            await _userRepository.UpdateUserAsync(user);
            return (true, "All tokens revoked successfully");
        }
        catch (Exception)
        {
            return (false, "Failed to revoke tokens");
        }
    }

    public async Task<(bool Success, string Message)> SendPasswordResetEmailAsync(string email)
    {
        var user = await _userRepository.GetUserByEmailAsync(email);

        if (user == null)
        {
            // Still return success to prevent email enumeration
            return (true, "If a user with this email exists, they will receive password reset instructions");
        }

        // Generate new reset token
        user.PasswordResetToken = GenerateRandomToken();
        user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1);

        // Revoke all existing refresh tokens
        var revokeResult = await RevokeAllUserTokensAsync(user.Id, "Password Reset");
        if (!revokeResult.Success)
        {
            return (false, "Failed to process password reset request");
        }

        await _userRepository.UpdateUserAsync(user);

        var resetLink = $"{_configuration["AppUrl"]}/reset-password?token={user.PasswordResetToken}&email={email}";
        await _emailService.SendPasswordResetEmailAsync(user, resetLink);

        return (true, "Password reset email sent");
    }

    public async Task<(bool Success, string Message)> VerifyEmailAsync(string email, string token)
    {
        var user = await _userRepository.GetUserByEmailAsync(email);

        if (user == null || user.EmailConfirmationToken != token || user.EmailConfirmationTokenExpires < DateTime.UtcNow)
        {
            return (false, "Invalid token");
        }

        user.IsEmailConfirmed = true;
        user.EmailConfirmationToken = null;
        user.EmailConfirmationTokenExpires = null;

        await _userRepository.UpdateUserAsync(user);

        return (true, "Email verified successfully");
    }

    public async Task<(bool Success, string Message)> DeleteSessionAsync(string refreshToken, string ipAddress)
    {
        var token = await _userRepository.GetRefreshTokenAsync(refreshToken);

        if (token == null)
            return (false, "Invalid token");

        if (!token.IsActive)
            return (false, "Inactive token");

        // Revoke token and save
        token.Revoked = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        await _userRepository.UpdateRefreshTokenAsync(token);

        return (true, "Session deleted successfully");
    }

    public async Task<User> GetUserAsync(int userId)
    {
        return await _userRepository.GetUserByIdAsync(userId);
    }

    private static string GenerateRandomToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
