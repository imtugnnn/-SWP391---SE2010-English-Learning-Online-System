using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.Services.Options;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace EnglishLearningOnlineSystem.Services.Implementations;

public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _options;

    public SmtpEmailSender(IOptions<SmtpOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
    {
        if (string.IsNullOrWhiteSpace(_options.Host) || string.IsNullOrWhiteSpace(_options.FromEmail))
        {
            throw new InvalidOperationException("SMTP settings are missing. Configure Smtp:Host and Smtp:FromEmail in appsettings.");
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromEmail, _options.FromName),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };
        message.To.Add(toEmail);

        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl
        };

        if (!string.IsNullOrWhiteSpace(_options.Username))
        {
            client.Credentials = new NetworkCredential(_options.Username, _options.Password);
        }

        await client.SendMailAsync(message);
    }
}
