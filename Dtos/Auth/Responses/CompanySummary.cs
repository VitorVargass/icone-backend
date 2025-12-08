namespace icone_backend.Dtos.Auth.Responses
{
    public class CompanySummary
    {
        public long? Id { get; set; }
        public string FantasyName { get; set; } = default!;
        public string CorporateName { get; set; } = default!;
        public string Plan { get; set; } = default!;
        public bool IsActive { get; set; }
    }
}
