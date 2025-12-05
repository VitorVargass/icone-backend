using icone_backend.Models;

namespace icone_backend.Dtos.Ingredient
{
    public class IngredientResponse
    {
        public int Id { get; set; }

        public IngredientScope Scope { get; set; }
        public long CreatedByUserId { get; set; }
        public Guid? CompanyId { get; set; }
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
        public double NonFatSolidsPct { get; set; }    // Non-fat solids
        public double MilkSolidsPct { get; set; }      // Milk solids
        public double OtherSolidsPct { get; set; }     // Other solids

    }
}
    