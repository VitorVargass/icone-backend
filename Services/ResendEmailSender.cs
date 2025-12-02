using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace icone_backend.Services
{
    public class ResendEmailSender : IEmailSender
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _fromEmail;

        public ResendEmailSender(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["Resend:ApiKey"] ?? throw new Exception("Resend ApiKey not configured");
            _fromEmail = config["Resend:FromEmail"] ?? "no-reply@icone.com";
        }

        public async Task SendTwoFactorCodeAsync(string toEmail, string code)
        {
            var url = "https://api.resend.com/emails";

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var body = new
            {
                from = _fromEmail,
                to = new[] { toEmail },
                subject = "Seu código de verificação - ICone",
                html = $"<p>Seu código de verificação é:</p><h2>{code}</h2><p>Ele expira em 10 minutos.</p>"
            };

            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erro ao enviar e-mail via Resend: {response.StatusCode} - {content}");
            }
        }


            public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
            {
            var url = "https://api.resend.com/emails";

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var body = new
            {
                from = _fromEmail,
                to = new[] { toEmail },
                subject = "Redefinição de senha - ICone",
                html = $"<p>Você solicitou redefinição de senha. Clique no link abaixo para criar uma nova senha:</p><p><a href=\"{resetLink}\">{resetLink}</a></p><p>Se você não solicitou, ignore este e-mail.</p>"
            };

            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erro ao enviar e-mail de redefinição via Resend: {response.StatusCode} - {content}");
            }
        }
    }
}
