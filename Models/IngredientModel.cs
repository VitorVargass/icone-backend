using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace icone_backend.Models
{
    public enum IngredientScope
    {
        System = 0,
        Company = 1,
        User = 2
    }
 
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AdditiveUsage
    {
        Hot,
        Cold,
        Both
    }

    [Owned]
    public class AdditiveScores
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

    public class AdditiveCompatibilityItem
    {
        public string Name { get; set; } = default!;
        public double Percentage { get; set; }
    }

    public class IngredientModel
    {
        [Key]
        public int Id { get; set; }

        public IngredientScope Scope { get; set; }

        [Column("created_by_user_id")]
        public long CreatedByUserId { get; set; }

        public Guid? CompanyId { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Category { get; set; } = null!;

        // ---- Nutritional composition ----
        public double? WaterPct { get; set; }
        public double? ProteinPct { get; set; }
        public double? CarbsPct { get; set; }
        public double? SugarPct { get; set; }
        public double? FiberPct { get; set; }
        public double? LactosePct { get; set; }
        public double? FatPct { get; set; }
        public double? FatSaturatedPct { get; set; }
        public double? FatMonounsaturatedPct { get; set; }
        public double? FatTransPct { get; set; }

        // ----- Technological parameters ----
        public double? AlcoholPct { get; set; }
        public double? Pod { get; set; }
        public double? Pac { get; set; }
        public double? KcalPer100g { get; set; }
        public double? SodiumMg { get; set; }
        public double? PotassiumMg { get; set; }
        public double? CholesterolMg { get; set; }

        public double? TotalSolidsPct { get; set; }
        public double? NonFatSolidsPct { get; set; }
        public double? MilkSolidsPct { get; set; }
        public double? OtherSolidsPct { get; set; }

        // Field Additives

        public double? MaxDoseGL { get; set; }

        
        public AdditiveUsage? Usage { get; set; }

        public string? Description { get; set; }

        public AdditiveScores? Scores { get; set; }

        public string? IncompatibleWithJson { get; set; }
        public string? CompatibleWithJson { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // ---- Helpers de (in)compatibilidade ----

        public List<string> GetIncompatibleWith()
        {
            if (string.IsNullOrWhiteSpace(IncompatibleWithJson))
                return new List<string>();

            return JsonSerializer.Deserialize<List<string>>(IncompatibleWithJson!)
                   ?? new List<string>();
        }

        public void SetIncompatibleWith(List<string>? values)
        {
            IncompatibleWithJson = values == null || values.Count == 0
                ? null
                : JsonSerializer.Serialize(values);
        }

        public List<AdditiveCompatibilityItem> GetCompatibleWith()
        {
            if (string.IsNullOrWhiteSpace(CompatibleWithJson))
                return new List<AdditiveCompatibilityItem>();

            return JsonSerializer.Deserialize<List<AdditiveCompatibilityItem>>(CompatibleWithJson!)
                   ?? new List<AdditiveCompatibilityItem>();
        }

        public void SetCompatibleWith(List<AdditiveCompatibilityItem>? values)
        {
            CompatibleWithJson = values == null || values.Count == 0
                ? null
                : JsonSerializer.Serialize(values);
        }
    }
}
