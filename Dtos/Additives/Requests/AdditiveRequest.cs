using icone_backend.Models;

namespace icone_backend.Dtos.Additives.Requests
{
    public class AdditiveRequest
    {
        public string Name { get; set; } = default!;
        public string Category { get; set; } = default!;

        public double? MaxDoseGL { get; set; }
        public AdditiveUsage Usage { get; set; }

        public string? Description { get; set; }

        public AdditiveScoresDto? Scores { get; set; }

        public List<string>? IncompatibleWith { get; set; }
        public List<AdditiveCompatibilityDto>? CompatibleWith { get; set; }
    }
}
