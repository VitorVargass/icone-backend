namespace icone_backend.Dtos.Neutral.Requests
{
    public class NeutralComponentRequest
    {
        public int AdditiveId { get; set; }
        public double QuantityPerLiter { get; set; }
    }

    public class CreateNeutralRequest
    {
        public string Name { get; set; } = null!;
        public string GelatoType { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty; 
        public double RecommendedDoseGPerKg { get; set; }

        public List<NeutralComponentRequest> Components { get; set; } = new();
    }
}
