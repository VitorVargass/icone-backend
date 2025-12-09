using icone_backend.Models;
using icone_backend.Dtos.Additives.Requests;

namespace icone_backend.Dtos.Additives.Responses
{
    public class AdditiveResponse
    {
        public int Id { get; set; }

        public string Name { get; set; } = default!;
        public string Category { get; set; } = default!;

        public int Scope { get; set; }
        public bool IsReadOnly { get; set; }

        public double? MaxDoseGL { get; set; }
        public AdditiveUsage Usage { get; set; }

        public string? Description { get; set; }

        public AdditiveScoresDto? Scores { get; set; }

        public List<string> IncompatibleWith { get; set; } = new();
        public List<AdditiveCompatibilityDto> CompatibleWith { get; set; } = new();

        public Guid? CompanyId { get; set; }
        public long? UserId { get; set; }
    }
}
