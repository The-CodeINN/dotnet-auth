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
}
