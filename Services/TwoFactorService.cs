using Microsoft.Extensions.Caching.Memory;

namespace icone_backend.Services
{
    public class TwoFactorService
    {
        private readonly IEmailSender _emailSender;
        private readonly IMemoryCache _cache;
        private readonly ILogger<TwoFactorService> _logger;

        private record TwoFactorEntry(long UserId, string Code);

        public TwoFactorService(
            IEmailSender emailSender,
            IMemoryCache cache,
            ILogger<TwoFactorService> logger)
        {
            _emailSender = emailSender;
            _cache = cache;
            _logger = logger;
        }

        // 2FA: código numérico (login)
        public async Task<string> GenerateAndSendCodeAsync(long userId, string email)
        {
            // código 6 dígitos
            var code = new Random().Next(100000, 999999).ToString();

           
            var twoFactorToken = Guid.NewGuid().ToString("N");

            var entry = new TwoFactorEntry(userId, code);

            _cache.Set(twoFactorToken, entry, TimeSpan.FromMinutes(10));

            await _emailSender.SendTwoFactorCodeAsync(email, code);
            _logger.LogInformation("Código 2FA enviado para {Email}", email);

            return twoFactorToken;
        }

        public Task<(bool Success, long UserId)> ValidateCodeAsync(string token, string code)
        {
            if (!_cache.TryGetValue<TwoFactorEntry>(token, out var entry))
            {
                // token inválido/expirado
                return Task.FromResult((false, 0L));
            }

            if (!string.Equals(entry.Code, code))
            {
                // código errado
                return Task.FromResult((false, 0L));
            }

            _cache.Remove(token);

            return Task.FromResult((true, entry.UserId));
        }

        // =========================
        // Reset de senha por link
        // =========================

        public string CacheResetToken(long userId, TimeSpan expiration)
        {
            var resetToken = Guid.NewGuid().ToString("N");
            _cache.Set($"reset_{resetToken}", userId, expiration);
            return resetToken;
        }

        
        public bool TryGetResetUserId(string token, out long userId)
        {
            if (_cache.TryGetValue<long>($"reset_{token}", out var id))
            {
                userId = id;
                return true;
            }

            userId = 0;
            return false;
        }

        public void RemoveResetToken(string token)
        {
            _cache.Remove($"reset_{token}");
        }
        public Task SendPasswordResetEmailAsync(string email, string resetLink)
        {
            return _emailSender.SendPasswordResetEmailAsync(email, resetLink);
        }
    }
}
