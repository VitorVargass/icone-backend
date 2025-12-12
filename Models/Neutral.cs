using System.Text.Json;

namespace icone_backend.Models
{

    public enum NeutralScope
    {
        System = 0,
        Company = 1,
        User = 2
    }

    public class Neutral
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
        public string GelatoType { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty; 
        public double RecommendedDoseGPerKg { get; set; }

        public double TotalDosagePerLiter { get; set; }

        public NeutralScope Scope { get; set; }
        public Guid? CompanyId { get; set; }
        public long CreatedByUserId { get; set; }


        public string? ComponentsJson { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

         

        public List<NeutralComponentItem> GetComponents()
        {
            if (string.IsNullOrWhiteSpace(ComponentsJson))
                return new List<NeutralComponentItem>();

            return JsonSerializer.Deserialize<List<NeutralComponentItem>>(ComponentsJson!)
                   ?? new List<NeutralComponentItem>();
        }

        public void SetComponents(List<NeutralComponentItem>? components)
        {
            ComponentsJson = components == null || components.Count == 0
                ? null
                : JsonSerializer.Serialize(components);
        }
    }

    
    public class NeutralComponentItem
    {
        public int IngredientId { get; set; }
        public string QuantityPerLiter { get; set; } = string.Empty;
    }
}
