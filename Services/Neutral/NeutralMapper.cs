using icone_backend.Dtos.Neutral.Responses;
using icone_backend.Models;

namespace icone_backend.Services.NeutralService
{
    public static class NeutralMapper
    {
        
        public static NeutralResponse ToResponse( this Neutral neutral, List<(IngredientModel ingredient, double quantityPerLiter)> components, NeutralMessagesDto messages)
        {
            return new NeutralResponse
            {
                Id = neutral.Id,
                Scope = neutral.Scope,
                Name = neutral.Name,
                GelatoType = neutral.GelatoType,
                Method = neutral.Method,
                RecommendedDoseGPerKg = neutral.RecommendedDoseGPerKg,
                TotalDosagePerLiter = neutral.TotalDosagePerLiter,

                Components = components.Select(c => new NeutralComponentDto
                {
                    IngredientId = c.ingredient.Id,
                    IngredientName = c.ingredient.Name,
                    QuantityPerLiter = c.quantityPerLiter
                }).ToList(),

                Messages = messages
            };
        }
    }
}
