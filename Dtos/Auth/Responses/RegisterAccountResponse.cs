namespace icone_backend.Dtos.Auth.Responses
{
    public class RegisterAccountResponse
    {
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
    }
}
