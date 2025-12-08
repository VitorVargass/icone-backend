namespace icone_backend.Dtos.Auth.Responses
{
    public class MeResult
    {
        public long Id { get; set; }
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Role { get; set; } = default!;
        public bool IsActive { get; set; }

        public long? CompanyId { get; set; }
        public CompanySummary? Company { get; set; }
    }
}
