using icone_backend.Dtos.Ingredient.Responses;
using icone_backend.Models;

namespace icone_backend.Dtos.Ingredient.Requests
{
    public class CreateIngredientRequest
    {
        public string Name { get; set; } = null!;
        public string Category { get; set; } = null!;

        // ---- Nutritional composition ----
        public double WaterPct { get; set; }
        public double ProteinPct { get; set; }
        public double CarbsPct { get; set; }
        public double SugarPct { get; set; }
        public double FiberPct { get; set; }
        public double LactosePct { get; set; }
        public double FatPct { get; set; }
        public double FatSaturatedPct { get; set; }
        public double FatMonounsaturatedPct { get; set; }
        public double FatTransPct { get; set; }

        // ----- Technological parameters ----
        public double AlcoholPct { get; set; }
        public double Pod { get; set; }
        public double Pac { get; set; }
        public double KcalPer100g { get; set; }
        public double SodiumMg { get; set; }
        public double PotassiumMg { get; set; }
        public double CholesterolMg { get; set; }

        public double TotalSolidsPct { get; set; }
        public double NonFatSolidsPct { get; set; }  
        public double MilkSolidsPct { get; set; } 
        public double OtherSolidsPct { get; set; }

        // ---- Campos de aditivo (se Type = Additive) ----
        public double? MaxDoseGL { get; set; }
        public AdditiveUsage? Usage { get; set; }
        public string? Description { get; set; }

        public AdditiveScoresDto? Scores { get; set; }

        public List<string> IncompatibleWith { get; set; } = new();
        public List<AdditiveCompatibilityItemDto> CompatibleWith { get; set; } = new();
    }
}
