namespace icone_backend.Dtos.Auth
{
    public class VerifyTwoFactorRequest
    {
        public string TwoFactorToken { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;

    }
}