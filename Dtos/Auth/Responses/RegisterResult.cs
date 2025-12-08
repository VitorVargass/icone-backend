using icone_backend.Models;

namespace icone_backend.Dtos.Auth.Responses
{
    public class RegisterResult
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public Error? Error { get; set; }
    }
}
