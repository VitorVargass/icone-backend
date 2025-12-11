using icone_backend.Models;

namespace icone_backend.Dtos.Ingredient.Responses
{
    public class AdditiveScoresDto
    {
        public double Stabilization { get; set; }
        public double Emulsifying { get; set; }
        public double LowPhResistance { get; set; }
        public double Creaminess { get; set; }
        public double Viscosity { get; set; }
        public double Body { get; set; }
        public double Elasticity { get; set; }
        public double Crystallization { get; set; }
    }

    public class AdditiveCompatibilityItemDto
    {
        public string Name { get; set; } = default!;
        public double Percentage { get; set; }
    }
}
