public interface IEmailSender
{
    Task SendTwoFactorCodeAsync(string toEmail, string code);
    Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
}
