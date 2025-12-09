using icone_backend.Models;

namespace icone_backend.Dtos.Neutral.Responses
{
    public class NeutralComponentDto
    {
        public int AdditiveId { get; set; }
        public string AdditiveName { get; set; } = string.Empty;
        public double QuantityPerLiter { get; set; }
    }

    public class NeutralMessagesDto
    {
        public List<string> Warnings { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }

    public class NeutralResponse
    {
        public int Id { get; set; }
        public NeutralScope Scope { get; set; }
        public string Name { get; set; } = null!;
        public string GelatoType { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;

        public double RecommendedDoseGPerKg { get; set; }
        public double TotalDosagePerLiter { get; set; }

        public List<NeutralComponentDto> Components { get; set; } = new();
        public NeutralMessagesDto Messages { get; set; } = new();
    }
}
