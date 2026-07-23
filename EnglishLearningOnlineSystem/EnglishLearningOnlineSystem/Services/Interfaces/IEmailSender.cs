//Create by TungDPL
//Last update: 7/21/2026
namespace EnglishLearningOnlineSystem.Services.Interfaces;

public interface IEmailSender
{
    Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
}
