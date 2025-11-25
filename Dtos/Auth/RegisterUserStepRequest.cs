namespace icone_backend.Dtos.Auth
{
    public class RegisterUserStepRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Document { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
