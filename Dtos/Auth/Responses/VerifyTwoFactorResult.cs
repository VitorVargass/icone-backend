using icone_backend.Models;

namespace icone_backend.Dtos.Auth.Responses
{
    public class VerifyTwoFactorResult
    {
        public bool Success { get; set; }
        public string? Jwt { get; set; }
        public Error? Error { get; set; }
    }
}
