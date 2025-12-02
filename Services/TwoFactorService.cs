using Microsoft.Extensions.Caching.Memory;

namespace icone_backend.Services
{
    public class TwoFactorService
    {
        private readonly IEmailSender _emailSender;
        private readonly IMemoryCache _cache;
        private readonly ILogger<TwoFactorService> _logger;

        // Registro que vai ficar no cache: qual usuário e qual código
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

        /// <summary>
        /// Gera um código de 6 dígitos, envia por e-mail e salva temporariamente no cache.
        /// Retorna um token (GUID em string) que o front vai usar depois no /verify-2fa.
        /// </summary>
        public async Task<string> GenerateAndSendCodeAsync(long userId, string email)
        {
            // código numérico de 6 dígitos
            var code = new Random().Next(100000, 999999).ToString();

            // token que o front vai guardar (não é o ID do usuário)
            var twoFactorToken = Guid.NewGuid().ToString("N");

            var entry = new TwoFactorEntry(userId, code);

            // salva no cache por 10 minutos
            _cache.Set(twoFactorToken, entry, TimeSpan.FromMinutes(10));

            // envia o e-mail com o código
            await _emailSender.SendTwoFactorCodeAsync(email, code);
            _logger.LogInformation("Código 2FA enviado para {Email}", email);

            return twoFactorToken;
        }

        /// <summary>
        /// Valida o código de verificação.
        /// Recebe o token (GUID que veio do login) e o código digitado.
        /// </summary>
        public Task<(bool Success, long UserId)> ValidateCodeAsync(string token, string code)
        {
            if (!_cache.TryGetValue<TwoFactorEntry>(token, out var entry))
            {
                // token inválido ou expirado
                return Task.FromResult((false, 0L));
            }

            if (!string.Equals(entry.Code, code))
            {
                // código errado
                return Task.FromResult((false, 0L));
            }

            // remove após uso (código só pode ser usado uma vez)
            _cache.Remove(token);

            return Task.FromResult((true, entry.UserId));
        }
    }
}
