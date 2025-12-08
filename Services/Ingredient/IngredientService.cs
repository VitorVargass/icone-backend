using icone_backend.Data;
using icone_backend.Interfaces;
using icone_backend.Models;
using icone_backend.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using icone_backend.Dtos.Ingredient.Requests;
using icone_backend.Dtos.Ingredient.Responses;

namespace icone_backend.Services.Ingredient
{
    public class IngredientService : IIngredientInterface
    {
        private readonly AppDbContext _context;
        private readonly IIngredientSolidsCalculator _ingredientSolidsCalculator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IngredientService(AppDbContext context, IIngredientSolidsCalculator ingredientSolidsCalculator, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _ingredientSolidsCalculator = ingredientSolidsCalculator;
            _httpContextAccessor = httpContextAccessor;
        }


        private long GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null)
            {
                throw new UnauthorizedAccessException("User not authenticated.");
            }

            var idStr =
                user.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(idStr) || !long.TryParse(idStr, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid user id in token.");
            }

            return userId;
        }

        public async Task<IEnumerable<IngredientResponse>> GetAllAsync(Guid? companyId)
        {
            var userId = GetCurrentUserId();

            var query = _context.Ingredients.AsQueryable();

            query = query.Where(i =>
                i.Scope == IngredientScope.System || // globais
                (i.Scope == IngredientScope.Company
                    && companyId.HasValue
                    && i.CompanyId == companyId)      // da empresa ativa
                ||
                (i.Scope == IngredientScope.User
                    && !companyId.HasValue
                    && i.CreatedByUserId == userId)   // pessoais do autônomo
            );

            return await query
                .Select(i => new IngredientResponse
                {
                    Id = i.Id,
                    Scope = i.Scope,
                    CompanyId = i.CompanyId,
                    Name = i.Name,
                    Category = i.Category,
                    CreatedByUserId = i.CreatedByUserId,

                    WaterPct = i.WaterPct,
                    ProteinPct  = i.ProteinPct,
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
                    OtherSolidsPct = i.OtherSolidsPct



                })
                .ToListAsync();
        }

        public async Task<IngredientResponse?> GetByIdAsync(int id)
        {
            var i = await _context.Ingredients.FindAsync(id);
            if (i == null) return null;

            var userId = GetCurrentUserId();

            return new IngredientResponse
            {
                Id = i.Id,
                Scope = i.Scope,
                CompanyId = i.CompanyId,
                Name = i.Name,
                Category = i.Category,
                CreatedByUserId = i.CreatedByUserId,

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
                OtherSolidsPct = i.OtherSolidsPct

            };
        }

        public async Task<IngredientResponse> CreateAsync(CreateIngredientRequest request, Guid? companyId)
        {
            var userId = GetCurrentUserId();

            _ingredientSolidsCalculator.CalculateIngredientSolids(request);

            var ingredient = new IngredientModel
            {
                
                Name = request.Name,
                Category = request.Category,
                CreatedByUserId = userId,


                WaterPct = request.WaterPct,
                ProteinPct = request.ProteinPct,
                CarbsPct = request.CarbsPct,
                SugarPct = request.SugarPct,
                FiberPct = request.FiberPct,
                LactosePct = request.LactosePct,
                FatPct = request.FatPct,
                FatSaturatedPct = request.FatSaturatedPct,
                FatMonounsaturatedPct = request.FatMonounsaturatedPct,
                FatTransPct = request.FatTransPct,

                AlcoholPct = request.AlcoholPct,
                Pod = request.Pod,
                Pac = request.Pac,
                KcalPer100g = request.KcalPer100g,
                SodiumMg = request.SodiumMg,
                PotassiumMg = request.PotassiumMg,
                CholesterolMg = request.CholesterolMg,


                TotalSolidsPct = request.TotalSolidsPct,
                NonFatSolidsPct = request.NonFatSolidsPct,
                MilkSolidsPct = request.MilkSolidsPct,
                OtherSolidsPct = request.OtherSolidsPct

            };

            if (companyId.HasValue)
            {
                ingredient.Scope = IngredientScope.Company;
                ingredient.CompanyId = companyId;
            }
            else
            {
                ingredient.Scope = IngredientScope.User;
                ingredient.CompanyId = null;
            }

            _context.Ingredients.Add(ingredient);
            await _context.SaveChangesAsync();

            return new IngredientResponse
            {
                Id = ingredient.Id,
                Name = ingredient.Name,
                Category = ingredient.Category,
                CreatedByUserId = ingredient.CreatedByUserId,
                Scope = ingredient.Scope,

                WaterPct = ingredient.WaterPct,
                ProteinPct = ingredient.ProteinPct,
                CarbsPct = ingredient.CarbsPct,
                SugarPct = ingredient.SugarPct,
                FiberPct = ingredient.FiberPct,
                LactosePct = ingredient.LactosePct,
                FatPct = ingredient.FatPct,
                FatSaturatedPct = ingredient.FatSaturatedPct,
                FatMonounsaturatedPct = ingredient.FatMonounsaturatedPct,
                FatTransPct = ingredient.FatTransPct,

                AlcoholPct = ingredient.AlcoholPct,
                Pod = ingredient.Pod,
                Pac = ingredient.Pac,
                KcalPer100g = ingredient.KcalPer100g,
                SodiumMg = ingredient.SodiumMg,
                PotassiumMg = ingredient.PotassiumMg,
                CholesterolMg = ingredient.CholesterolMg,


                TotalSolidsPct = ingredient.TotalSolidsPct,
                NonFatSolidsPct = ingredient.NonFatSolidsPct,
                MilkSolidsPct = ingredient.MilkSolidsPct,
                OtherSolidsPct = ingredient.OtherSolidsPct

            };
        }

        public async Task<IngredientResponse?> UpdateAsync(int id, UpdateIngredientRequest request)
        {
            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null) return null;

            var userId = GetCurrentUserId();

            _ingredientSolidsCalculator.CalculateIngredientSolids(request);


            ingredient.Name = request.Name;
            ingredient.Category = request.Category;
            

            ingredient.WaterPct = request.WaterPct;
            ingredient.ProteinPct = request.ProteinPct;
            ingredient.CarbsPct = request.CarbsPct;
            ingredient.SugarPct = request.SugarPct;
            ingredient.FiberPct = request.FiberPct;
            ingredient.LactosePct = request.LactosePct;
            ingredient.FatPct = request.FatPct;
            ingredient.FatSaturatedPct = request.FatSaturatedPct;
            ingredient.FatMonounsaturatedPct = request.FatMonounsaturatedPct;
            ingredient.FatTransPct = request.FatTransPct;

            ingredient.AlcoholPct = request.AlcoholPct;
            ingredient.Pod = request.Pod;
            ingredient.Pac = request.Pac;
            ingredient.KcalPer100g = request.KcalPer100g;
            ingredient.SodiumMg = request.SodiumMg;
            ingredient.PotassiumMg = request.PotassiumMg;
            ingredient.CholesterolMg = request.CholesterolMg;

            ingredient.TotalSolidsPct = request.TotalSolidsPct;
            ingredient.NonFatSolidsPct = request.NonFatSolidsPct;
            ingredient.MilkSolidsPct = request.MilkSolidsPct;
            ingredient.OtherSolidsPct = request.OtherSolidsPct;


            await _context.SaveChangesAsync();

            return new IngredientResponse
            {
                Id = ingredient.Id,
                Name = ingredient.Name,
                Category = ingredient.Category,
                CreatedByUserId = ingredient.CreatedByUserId,

                WaterPct = ingredient.WaterPct,
                ProteinPct = ingredient.ProteinPct,
                CarbsPct = ingredient.CarbsPct,
                SugarPct = ingredient.SugarPct,
                FiberPct = ingredient.FiberPct,
                LactosePct = ingredient.LactosePct,
                FatPct = ingredient.FatPct,
                FatSaturatedPct = ingredient.FatSaturatedPct,
                FatMonounsaturatedPct = ingredient.FatMonounsaturatedPct,
                FatTransPct = ingredient.FatTransPct,

                AlcoholPct = ingredient.AlcoholPct,
                Pod = ingredient.Pod,
                Pac = ingredient.Pac,
                KcalPer100g = ingredient.KcalPer100g,
                SodiumMg = ingredient.SodiumMg,
                PotassiumMg = ingredient.PotassiumMg,
                CholesterolMg = ingredient.CholesterolMg,


                TotalSolidsPct = ingredient.TotalSolidsPct,
                NonFatSolidsPct = ingredient.NonFatSolidsPct,
                MilkSolidsPct = ingredient.MilkSolidsPct,
                OtherSolidsPct = ingredient.OtherSolidsPct

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
    }
}