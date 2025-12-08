using icone_backend.Models;

namespace icone_backend.Dtos.Auth.Responses
{
    public class ResetPasswordResult
    {
        public bool Success { get; set; }
        public Error? Error { get; set; }
    }
}
