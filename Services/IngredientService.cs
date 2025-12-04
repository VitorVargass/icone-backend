using icone_backend.Data;
using icone_backend.Dtos.Ingredient;
using icone_backend.Dtos.Ingridient;
using icone_backend.Interfaces;
using icone_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace icone_backend.Services
{
    public class IngredientService : IIngredientService
    {
        private readonly AppDbContext _context;

        public IngredientService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<IngredientResponse>> GetAllAsync()
        {
            return await _context.Ingredients
                .Select(i => new IngredientResponse
                {
                    Id = i.Id,
                    Name = i.Name,
                    Category = i.Category,
                    TotalSolidsPct = i.TotalSolidsPct,
                    Pac = i.Pac,
                    Pod = i.Pod,
                    KcalPer100g = i.KcalPer100g
                })
                .ToListAsync();
        }

        public async Task<IngredientResponse?> GetByIdAsync(int id)
        {
            var i = await _context.Ingredients.FindAsync(id);
            if (i == null) return null;

            return new IngredientResponse
            {
                Id = i.Id,
                Name = i.Name,
                Category = i.Category,
                TotalSolidsPct = i.TotalSolidsPct,
                Pac = i.Pac,
                Pod = i.Pod,
                KcalPer100g = i.KcalPer100g
            };
        }

        public async Task<IngredientResponse> CreateAsync(CreateIngredientRequest request)
        {
            var ingredient = new IngredientModel
            {
                Name = request.Name,
                Category = request.Category,
                WaterPct = request.WaterPct,
                FatPct = request.FatPct,
                ProteinPct = request.ProteinPct,
                SugarPct = request.SugarPct,
                LactosePct = request.LactosePct,
                FiberPct = request.FiberPct,
                CarbsPct = request.CarbsPct,
                AlcoholPct = request.AlcoholPct,
                TotalSolidsPct = CalcTotalSolids(request),
                Pac = request.Pac,
                Pod = request.Pod,
                KcalPer100g = request.KcalPer100g
            };

            _context.Ingredients.Add(ingredient);
            await _context.SaveChangesAsync();

            return new IngredientResponse
            {
                Id = ingredient.Id,
                Name = ingredient.Name,
                Category = ingredient.Category,
                TotalSolidsPct = ingredient.TotalSolidsPct,
                Pac = ingredient.Pac,
                Pod = ingredient.Pod,
                KcalPer100g = ingredient.KcalPer100g
            };
        }

        public async Task<IngredientResponse?> UpdateAsync(int id, UpdateIngredientRequest request)
        {
            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null) return null;

            ingredient.Name = request.Name;
            ingredient.Category = request.Category;
            ingredient.WaterPct = request.WaterPct;
            ingredient.FatPct = request.FatPct;
            ingredient.ProteinPct = request.ProteinPct;
            ingredient.SugarPct = request.SugarPct;
            ingredient.LactosePct = request.LactosePct;
            ingredient.FiberPct = request.FiberPct;
            ingredient.CarbsPct = request.CarbsPct;
            ingredient.AlcoholPct = request.AlcoholPct;
            ingredient.TotalSolidsPct = CalcTotalSolids(request);
            ingredient.Pac = request.Pac;
            ingredient.Pod = request.Pod;
            ingredient.KcalPer100g = request.KcalPer100g;

            await _context.SaveChangesAsync();

            return new IngredientResponse
            {
                Id = ingredient.Id,
                Name = ingredient.Name,
                Category = ingredient.Category,
                TotalSolidsPct = ingredient.TotalSolidsPct,
                Pac = ingredient.Pac,
                Pod = ingredient.Pod,
                KcalPer100g = ingredient.KcalPer100g
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null) return false;

            _context.Ingredients.Remove(ingredient);
            await _context.SaveChangesAsync();
            return true;
        }

        private double CalcTotalSolids(CreateIngredientRequest i)
        {
            return Math.Round(100 - i.WaterPct, 2);
        }
    }
}