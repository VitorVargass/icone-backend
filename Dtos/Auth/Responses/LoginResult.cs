using icone_backend.Models;

namespace icone_backend.Dtos.Auth.Responses
{
    public class LoginResult
    {
        public bool Success { get; set; }
        public bool Requires2FA { get; set; }
        public string? TwoFactorToken { get; set; }
        public Error? Error { get; set; }
    }
}
