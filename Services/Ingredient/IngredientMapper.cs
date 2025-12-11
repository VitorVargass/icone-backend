using icone_backend.Dtos.Ingredient.Responses;
using icone_backend.Models;

namespace icone_backend.Services.Ingredient
{
    public static class IngredientMapper
    {
        public static IngredientResponse ToResponse(this IngredientModel i)
        {
            var incompatible = i.GetIncompatibleWith();
            var compatible = i.GetCompatibleWith();

            return new IngredientResponse
            {
                Id = i.Id,
                Scope = i.Scope,
                CreatedByUserId = i.CreatedByUserId,
                CompanyId = i.CompanyId,
                Name = i.Name,
                Category = i.Category,

                WaterPct = i.WaterPct,
                ProteinPct = i.ProteinPct,
                CarbsPct = i.CarbsPct,
                SugarPct = i.SugarPct,
                FiberPct = i.FiberPct,
                LactosePct = i.LactosePct,
                FatPct = i.FatPct,
                FatSaturatedPct = i.FatSaturatedPct,
                FatMonounsaturatedPct = i.FatMonounsaturatedPct,
                FatTransPct = i.FatTransPct,

                AlcoholPct = i.AlcoholPct,
                Pod = i.Pod,
                Pac = i.Pac,
                KcalPer100g = i.KcalPer100g,
                SodiumMg = i.SodiumMg,
                PotassiumMg = i.PotassiumMg,
                CholesterolMg = i.CholesterolMg,

                TotalSolidsPct = i.TotalSolidsPct,
                NonFatSolidsPct = i.NonFatSolidsPct,
                MilkSolidsPct = i.MilkSolidsPct,
                OtherSolidsPct = i.OtherSolidsPct,

                MaxDoseGL = i.MaxDoseGL,
                Usage = i.Usage,
                Description = i.Description,

                Scores = i.Scores is null
                    ? null
                    : new AdditiveScoresDto
                    {
                        Stabilization = i.Scores.Stabilization,
                        Emulsifying = i.Scores.Emulsifying,
                        LowPhResistance = i.Scores.LowPhResistance,
                        Creaminess = i.Scores.Creaminess,
                        Viscosity = i.Scores.Viscosity,
                        Body = i.Scores.Body,
                        Elasticity = i.Scores.Elasticity,
                        Crystallization = i.Scores.Crystallization
                    },

                IncompatibleWith = incompatible,
                CompatibleWith = compatible
                    .Select(c => new AdditiveCompatibilityItemDto
                    {
                        Name = c.Name,
                        Percentage = c.Percentage
                    })
                    .ToList(),

                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt
            };
        }
    }
}
