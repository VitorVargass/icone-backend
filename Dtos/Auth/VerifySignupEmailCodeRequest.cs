namespace icone_backend.Dtos.Auth
{
    public class VerifySignupEmailCodeRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
