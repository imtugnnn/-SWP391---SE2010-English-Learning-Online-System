//Create by TungDPL
//Last update: 7/21/2026
using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.Services.Models;
using EnglishLearningOnlineSystem.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Cryptography;

namespace EnglishLearningOnlineSystem.Services.Implementations;

public class PasswordResetService : IPasswordResetService
{
    //BR16: Password token is only valid for 15 minutes
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(15);

    private readonly AppDbContext _db;
    private readonly IEmailSender _emailSender;

    public PasswordResetService(AppDbContext db, IEmailSender emailSender)
    {
        _db = db;
        _emailSender = emailSender;
    }

    public async Task RequestPasswordResetAsync(ForgotPasswordViewModel model, Func<string, string> resetUrlFactory)
    {
        var normalizedEmail = model.Email.Trim().ToLower();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

        if (user == null || !user.IsActive)
        {
            return;
        }

        var oldTokens = await _db.PasswordResetTokens
            .Where(t => t.UserId == user.Id)
            .ToListAsync();
        _db.PasswordResetTokens.RemoveRange(oldTokens);

        var passwordResetToken = new PasswordResetToken
        {
            UserId = user.Id,
            Token = GenerateToken(),
            CreatedAt = DateTime.UtcNow
        };

        _db.PasswordResetTokens.Add(passwordResetToken);
        await _db.SaveChangesAsync();

        var resetUrl = resetUrlFactory(passwordResetToken.Token);
        var encodedUrl = WebUtility.HtmlEncode(resetUrl);
        var htmlMessage = $@"
<p>Hello {WebUtility.HtmlEncode(user.Username)},</p>
<p>Use the link below to reset your password. This link is valid for 15 minutes.</p>
<p><a href=""{encodedUrl}"">Reset your password</a></p>
<p>If you did not request this, you can ignore this email.</p>";

        await _emailSender.SendEmailAsync(user.Email, "Reset your password", htmlMessage);
    }
    
    //BR16: Password token is only valid for 15 minutes
    public async Task<AuthServiceResult> ResetPasswordAsync(ResetPasswordViewModel model)
    {
        var token = await _db.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == model.Token);

        if (token == null)
        {
            return AuthServiceResult.Failure((string.Empty, "Đường link không hợp lệ"));
        }

        //BR16: Password token is only valid for 15 minutes
        if (DateTime.UtcNow - token.CreatedAt > TokenLifetime)
        {
            _db.PasswordResetTokens.Remove(token);
            await _db.SaveChangesAsync();
            return AuthServiceResult.Failure((string.Empty, "Password reset link has expired."));
        }

        if (!token.User.IsActive)
        {
            return AuthServiceResult.Failure((string.Empty, "Your account is inactive. Please contact support."));
        }

        if (string.IsNullOrWhiteSpace(model.Password))
        {
            return AuthServiceResult.Failure((nameof(model.Password), "Mật khẩu không được để trống."));
        }

        token.User.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
        _db.PasswordResetTokens.Remove(token);
        await _db.SaveChangesAsync();

        return AuthServiceResult.Success();
    }

    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
