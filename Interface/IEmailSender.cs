public interface IEmailSender
{
    Task SendTwoFactorCodeAsync(string toEmail, string code);
}
