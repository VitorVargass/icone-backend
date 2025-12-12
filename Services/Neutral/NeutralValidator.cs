using icone_backend.Dtos.Neutral;
using icone_backend.Dtos.Neutral.Responses;
using icone_backend.Models;

namespace icone_backend.Services.NeutralService
{
    public class NeutralValidator
    {
        public NeutralMessagesDto Validate( Neutral neutral, List<(IngredientModel ingredient, double quantityPerLiter)> components)
        {
            var messages = new NeutralMessagesDto();

            var total = components.Sum(c => c.quantityPerLiter);
            neutral.TotalDosagePerLiter = total;

            
            if (total < 5.0)
                messages.Warnings.Add($"Total = {total:0.###} g/L → Recommended 5 g/L.");
            else if (total > 5.0)
                messages.Warnings.Add($"Total = {total:0.###} g/L → Maximum is 5 g/L.");

            // MaxDose Validation (bloqueante)
            foreach (var comp in components)
            {
                var i = comp.ingredient;

                if (i.MaxDoseGL.HasValue && comp.quantityPerLiter > i.MaxDoseGL.Value)
                {
                    messages.Errors.Add(
                        $"Ingredient '{i.Name}': {comp.quantityPerLiter:0.###} g/L exceeds max dose {i.MaxDoseGL:0.###} g/L."
                    );
                }
            }

            // Method HOT/COLD/BOTH (warning)
            var method = neutral.Method?.Trim().ToLowerInvariant();
            foreach (var comp in components)
            {
                var i = comp.ingredient;

                if (string.IsNullOrWhiteSpace(method)) continue;

                if (method == "hot" && i.Usage == AdditiveUsage.Cold)
                {
                    messages.Warnings.Add(
                        $"Ingredient '{i.Name}' is marked as Cold but neutral method is Hot."
                    );
                }

                if (method == "cold" && i.Usage == AdditiveUsage.Hot)
                {
                    messages.Warnings.Add(
                        $"Ingredient '{i.Name}' is marked as Hot but neutral method is Cold."
                    );
                }
            }

            // Incompatibles (bloqueante) — mantendo a mesma lógica do seu código atual
            for (int x = 0; x < components.Count; x++)
            {
                for (int y = x + 1; y < components.Count; y++)
                {
                    var a = components[x].ingredient;
                    var b = components[y].ingredient;

                    var aIncompat = a.GetIncompatibleWith();
                    var bIncompat = b.GetIncompatibleWith();

                    if (aIncompat.Any(n => string.Equals(n, b.Name, StringComparison.OrdinalIgnoreCase)) ||
                        bIncompat.Any(n => string.Equals(n, a.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        messages.Errors.Add(
                            $"Ingredients '{a.Name}' and '{b.Name}' are marked as incompatible."
                        );
                    }
                }
            }

            return messages;
        }
    }
}
