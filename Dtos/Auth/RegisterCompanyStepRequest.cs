namespace icone_backend.Dtos.Auth
{
    public class RegisterCompanyStepRequest
    {
        public long UserId { get; set; }
        public string Document { get; set; } = null!;
        public string FantasyName { get; set; } = null!;
        public string CorporateName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? Website { get; set; }
    }
}
