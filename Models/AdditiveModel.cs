using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace icone_backend.Models
{
    
    public enum AdditiveScope
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

    public class AdditiveModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = default!;

        [Required]
        public string Category { get; set; } = default!;

        public AdditiveScope Scope { get; set; }

        public double? MaxDoseGL { get; set; }

        [Required]
        public AdditiveUsage Usage { get; set; }

        public string? Description { get; set; }

        public AdditiveScores? Scores { get; set; }

        

        public string? IncompatibleWithJson { get; set; }
        public string? CompatibleWithJson { get; set; }

        public Guid? CompanyId { get; set; }
        public long CreatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        
        public List<string> GetIncompatibleWith()
        {
            if (string.IsNullOrWhiteSpace(IncompatibleWithJson))
                return new List<string>();

            return JsonSerializer.Deserialize<List<string>>(IncompatibleWithJson!) ?? new List<string>();
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

    // Essa classe é só pra ajudar no JSON da tabela
    public class AdditiveCompatibilityItem
    {
        public string Name { get; set; } = default!;
        public double Percentage { get; set; }
    }
}
