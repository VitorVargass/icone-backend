using icone_backend.Dtos.Ingridient;
using System;
using System.Globalization;

namespace icone_backend.Services
{
    public interface IIngredientSolidsCalculator
    {
        void CalculateIngredientSolids(CreateIngredientRequest ingredient);
    }

    public class IngredientSolidsCalculator : IIngredientSolidsCalculator
    {
        public void CalculateIngredientSolids(CreateIngredientRequest ingredient)
        {
            
            double water = ingredient.WaterPct;
            double fat = ingredient.FatPct;
            double protein = ingredient.ProteinPct;
            double sugar = ingredient.SugarPct;
            double fiber = ingredient.FiberPct;
            double lactose = ingredient.LactosePct;
            double carbohydrates = ingredient.CarbsPct;

            string category = (ingredient.Category ?? string.Empty)
                .ToLowerInvariant();

            bool isDairyCategory =
                category.Contains("latte") ||
                category.Contains("panna") ||
                category.Contains("yogurt") ||
                category.Contains("formaggio");

           
            if (lactose == 0 && isDairyCategory)
            {
                lactose = sugar;
            }

            bool allZero =
                water == 0 &&
                fat == 0 &&
                protein == 0 &&
                sugar == 0 &&
                fiber == 0 &&
                lactose == 0 &&
                carbohydrates == 0;

            if (allZero)
            {
                ingredient.LactosePct = 0;
                ingredient.TotalSolidsPct = 0;
                ingredient.NonFatSolidsPct = 0;
                ingredient.MilkSolidsPct = 0;
                ingredient.OtherSolidsPct = 0;

                return;
            }

           

           
            double totalSolids = Math.Round(100 - water, 2);

           
            double nonFatSolids = Math.Round(
                protein + sugar + lactose + fiber + carbohydrates,
                2
            );

           
            double milkSolids = isDairyCategory
                ? Math.Round(protein + lactose, 2)
                : 0.0;

           
            double otherSolids =
                totalSolids - (fat + protein + sugar + lactose + fiber + carbohydrates);

            if (otherSolids < 0)
                otherSolids = 0;

            otherSolids = Math.Round(otherSolids, 2);

            // Update DTO
            ingredient.LactosePct = Math.Round(lactose, 2);
            ingredient.TotalSolidsPct = totalSolids; // Solidos totais
            ingredient.NonFatSolidsPct = nonFatSolids; //Solidos com baixo teor de gordura
            ingredient.MilkSolidsPct = milkSolids; // Solidos do leite
            ingredient.OtherSolidsPct = otherSolids; //Outros solidos



        }
    }
}
