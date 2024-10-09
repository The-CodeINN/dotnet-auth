using System;
using dotnet_auth.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace dotnet_auth.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendWelcomeEmailAsync(User user);
    Task SendConfirmationEmailAsync(User user, string confirmationLink);
    Task SendPasswordResetEmailAsync(User user, string resetLink);
    Task SendPasswordResetSuccessEmailAsync(User user);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var emailSettings = _configuration.GetSection("EmailSettings");
        Console.WriteLine(emailSettings["SmtpServer"]);
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(emailSettings["FromName"], emailSettings["FromEmail"]));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        var builder = new BodyBuilder();
        builder.HtmlBody = body;
        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(emailSettings["SmtpServer"], int.Parse(emailSettings["SmtpPort"]), SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(emailSettings["SmtpUsername"], emailSettings["SmtpPassword"]);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    public async Task SendWelcomeEmailAsync(User user)
    {
        var subject = "Welcome to Our Platform!";
        var body = EmailService.GenerateWelcomeEmailHtml(user);
        await SendEmailAsync(user.Email, subject, body);
    }

    public async Task SendConfirmationEmailAsync(User user, string confirmationLink)
    {
        var subject = "Confirm Your Email Address";
        var body = EmailService.GenerateConfirmationEmailHtml(user, confirmationLink);
        await SendEmailAsync(user.Email, subject, body);
    }

    public async Task SendPasswordResetEmailAsync(User user, string resetLink)
    {
        var subject = "Reset Your Password";
        var body = GeneratePasswordResetEmailHtml(user, resetLink);
        await SendEmailAsync(user.Email, subject, body);
    }

    public async Task SendPasswordResetSuccessEmailAsync(User user)
    {
        var subject = "Your Password Has Been Reset Successfully";
        var body = GeneratePasswordResetSuccessEmailHtml(user, _configuration);
        await SendEmailAsync(user.Email, subject, body);
    }

    private static string GenerateWelcomeEmailHtml(User user)
    {
        return $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Welcome to Our Platform</title>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        line-height: 1.6;
                        color: #333;
                        max-width: 600px;
                        margin: 0 auto;
                        padding: 20px;
                    }}
                    h1 {{
                        color: #4a4a4a;
                    }}
                    .cta-button {{
                        display: inline-block;
                        padding: 10px 20px;
                        background-color: #007bff;
                        color: #ffffff;
                        text-decoration: none;
                        border-radius: 5px;
                        margin-top: 20px;
                    }}
                </style>
            </head>
            <body>
                <h1>Welcome to Our Platform, {user.FirstName}!</h1>
                <p>We're excited to have you on board. Here are a few things you can do to get started:</p>
                <ul>
                    <li>Complete your profile</li>
                    <li>Explore our features</li>
                    <li>Connect with other users</li>
                </ul>
                <p>If you have any questions, feel free to reach out to our support team.</p>
                <a href='#' class='cta-button'>Get Started</a>
                <p>Best regards,<br>The Team</p>
            </body>
            </html>";
    }

    private static string GenerateConfirmationEmailHtml(User user, string confirmationLink)
    {
        return $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Confirm Your Email Address</title>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        line-height: 1.6;
                        color: #333;
                        max-width: 600px;
                        margin: 0 auto;
                        padding: 20px;
                    }}
                    h1 {{
                        color: #4a4a4a;
                    }}
                    .cta-button {{
                        display: inline-block;
                        padding: 10px 20px;
                        background-color: #28a745;
                        color: #ffffff;
                        text-decoration: none;
                        border-radius: 5px;
                        margin-top: 20px;
                    }}
                </style>
            </head>
            <body>
                <h1>Confirm Your Email Address</h1>
                <p>Hi {user.FirstName},</p>
                <p>Thank you for registering with our platform. To complete your registration, please confirm your email address by clicking the button below:</p>
                <a href='{confirmationLink}' class='cta-button'>Confirm Email</a>
                <p>If you didn't create an account with us, you can safely ignore this email.</p>
                <p>Best regards,<br>The Team</p>
            </body>
            </html>";
    }

    private static string GeneratePasswordResetEmailHtml(User user, string resetLink)
    {
        return $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Reset Your Password</title>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        line-height: 1.6;
                        color: #333;
                        max-width: 600px;
                        margin: 0 auto;
                        padding: 20px;
                        background-color: #f9f9f9;
                    }}
                    .email-container {{
                        background-color: #ffffff;
                        padding: 30px;
                        border-radius: 8px;
                        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
                    }}
                    h1 {{
                        color: #2c3e50;
                        margin-bottom: 20px;
                        font-size: 24px;
                    }}
                    .reset-button {{
                        display: inline-block;
                        padding: 12px 24px;
                        background-color: #e74c3c;
                        color: #ffffff;
                        text-decoration: none;
                        border-radius: 5px;
                        margin: 20px 0;
                        font-weight: bold;
                    }}
                    .security-notice {{
                        margin-top: 20px;
                        padding: 15px;
                        background-color: #f8f9fa;
                        border-left: 4px solid #2ecc71;
                        font-size: 14px;
                    }}
                    .expiry-notice {{
                        color: #7f8c8d;
                        font-size: 14px;
                        margin-top: 15px;
                    }}
                </style>
            </head>
            <body>
                <div class='email-container'>
                    <h1>Password Reset Request</h1>
                    <p>Hi {user.FirstName},</p>
                    <p>We received a request to reset the password for your account. If you made this request, please click the button below to reset your password:</p>
                    
                    <a href='{resetLink}' class='reset-button'>Reset Password</a>
                    
                    <p class='expiry-notice'>This password reset link will expire in 1 hour for security reasons.</p>
                    
                    <div class='security-notice'>
                        <strong>Security Notice:</strong>
                        <p>For your security, we have logged out all active sessions on your account. If you didn't request this password reset, please:</p>
                        <ul>
                            <li>Change your password immediately</li>
                            <li>Contact our support team</li>
                            <li>Review your account activity</li>
                        </ul>
                    </div>
                    
                    <p>Best regards,<br>The Team</p>
                </div>
            </body>
            </html>";
    }

    private static string GeneratePasswordResetSuccessEmailHtml(User user, IConfiguration configuration)
    {
        return $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Password Reset Successful</title>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        line-height: 1.6;
                        color: #333;
                        max-width: 600px;
                        margin: 0 auto;
                        padding: 20px;
                        background-color: #f9f9f9;
                    }}
                    .email-container {{
                        background-color: #ffffff;
                        padding: 30px;
                        border-radius: 8px;
                        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
                    }}
                    h1 {{
                        color: #2c3e50;
                        margin-bottom: 20px;
                        font-size: 24px;
                    }}
                    .success-icon {{
                        text-align: center;
                        margin: 20px 0;
                        font-size: 48px;
                        color: #2ecc71;
                    }}
                    .login-button {{
                        display: inline-block;
                        padding: 12px 24px;
                        background-color: #2ecc71;
                        color: #ffffff;
                        text-decoration: none;
                        border-radius: 5px;
                        margin: 20px 0;
                        font-weight: bold;
                    }}
                    .security-notice {{
                        margin-top: 20px;
                        padding: 15px;
                        background-color: #f8f9fa;
                        border-left: 4px solid #3498db;
                        font-size: 14px;
                    }}
                    .device-info {{
                        color: #7f8c8d;
                        font-size: 14px;
                        margin-top: 15px;
                    }}
                </style>
            </head>
            <body>
                <div class='email-container'>
                    <div class='success-icon'>✓</div>
                    <h1>Password Successfully Reset</h1>
                    <p>Hi {user.FirstName},</p>
                    <p>Your password has been successfully reset. For your security, we've logged out all active sessions on your account.</p>
                    
                    <a href='{configuration["AppUrl"]}/login' class='login-button'>Log In Now</a>
                    
                    <div class='security-notice'>
                        <strong>Important Security Information:</strong>
                        <ul>
                            <li>All existing sessions have been logged out</li>
                            <li>You'll need to log in again on all your devices</li>
                            <li>Any saved passwords or automatic logins should be updated</li>
                        </ul>
                    </div>

                    <div class='device-info'>
                        <p>If you did not make this change, please:</p>
                        <ol>
                            <li>Contact our support team immediately</li>
                            <li>Review your recent account activity</li>
                            <li>Consider enabling two-factor authentication</li>
                        </ol>
                    </div>
                    
                    <p>Best regards,<br>The Team</p>
                </div>
            </body>
            </html>";
    }
}
