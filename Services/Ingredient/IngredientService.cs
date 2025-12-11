using icone_backend.Data;
using icone_backend.Dtos.Ingredient.Requests;
using icone_backend.Dtos.Ingredient.Responses;
using icone_backend.Interface;
using icone_backend.Interfaces;
using icone_backend.Models;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace icone_backend.Services.Ingredient
{
    public class IngredientService : IIngredientInterface
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IngredientService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private long GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User
                ?? throw new UnauthorizedAccessException("User not authenticated.");

            var idStr =
                user.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(idStr) || !long.TryParse(idStr, out var userId))
                throw new UnauthorizedAccessException("Invalid user id in token.");

            return userId;
        }

        // ----------------- READ -----------------

        public async Task<IEnumerable<IngredientResponse>> GetAllAsync(Guid? companyId)
        {
            var userId = GetCurrentUserId();

            var query = _context.Ingredients.AsQueryable();

            query = query.Where(i =>
                i.Scope == IngredientScope.System ||
                (i.Scope == IngredientScope.Company &&
                    companyId.HasValue &&
                    i.CompanyId == companyId) ||
                (i.Scope == IngredientScope.User &&
                    !companyId.HasValue &&
                    i.CreatedByUserId == userId)
            );

            var list = await query.AsNoTracking().ToListAsync();
            return list.Select(x => x.ToResponse());
        }

        public async Task<IngredientResponse?> GetByIdAsync(int id)
        {
            var i = await _context.Ingredients.FindAsync(id);
            return i?.ToResponse();
        }

        // ----------------- CREATE -----------------

        public async Task<IngredientResponse> CreateAsync(CreateIngredientRequest request, Guid? companyId)
        {
            var userId = GetCurrentUserId();

            var model = new IngredientModel
            {
                Name = request.Name,
                Category = request.Category,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            // Scope
            if (companyId.HasValue)
            {
                model.Scope = IngredientScope.Company;
                model.CompanyId = companyId;
            }
            else
            {
                model.Scope = IngredientScope.User;
                model.CompanyId = null;
            }

            // Nutritional
            model.WaterPct = request.WaterPct;
            model.ProteinPct = request.ProteinPct;
            model.CarbsPct = request.CarbsPct;
            model.SugarPct = request.SugarPct;
            model.FiberPct = request.FiberPct;
            model.LactosePct = request.LactosePct;
            model.FatPct = request.FatPct;
            model.FatSaturatedPct = request.FatSaturatedPct;
            model.FatMonounsaturatedPct = request.FatMonounsaturatedPct;
            model.FatTransPct = request.FatTransPct;

            // Tech
            model.AlcoholPct = request.AlcoholPct;
            model.Pod = request.Pod;
            model.Pac = request.Pac;
            model.KcalPer100g = request.KcalPer100g;
            model.SodiumMg = request.SodiumMg;
            model.PotassiumMg = request.PotassiumMg;
            model.CholesterolMg = request.CholesterolMg;

            model.TotalSolidsPct = request.TotalSolidsPct;
            model.NonFatSolidsPct = request.NonFatSolidsPct;
            model.MilkSolidsPct = request.MilkSolidsPct;
            model.OtherSolidsPct = request.OtherSolidsPct;

            if (request.Category.Equals("Aditivos", StringComparison.OrdinalIgnoreCase))
            {
                // Additive
                model.MaxDoseGL = request.MaxDoseGL;
                model.Usage = request.Usage;
                model.Description = request.Description;

                if (request.Scores is not null)
                {
                    model.Scores = new AdditiveScores
                    {
                        Stabilization = request.Scores.Stabilization,
                        Emulsifying = request.Scores.Emulsifying,
                        LowPhResistance = request.Scores.LowPhResistance,
                        Creaminess = request.Scores.Creaminess,
                        Viscosity = request.Scores.Viscosity,
                        Body = request.Scores.Body,
                        Elasticity = request.Scores.Elasticity,
                        Crystallization = request.Scores.Crystallization
                    };
                }
            }

            model.SetIncompatibleWith(request.IncompatibleWith ?? new List<string>());

            if (request.CompatibleWith is not null)
            {
                var compList = request.CompatibleWith
                    .Select(c => new AdditiveCompatibilityItem
                    {
                        Name = c.Name,
                        Percentage = c.Percentage
                    })
                    .ToList();

                model.SetCompatibleWith(compList);
            }
            else
            {
                model.SetCompatibleWith(new List<AdditiveCompatibilityItem>());
            }

            _context.Ingredients.Add(model);
            await _context.SaveChangesAsync();

            return model.ToResponse();
        }

        // ----------------- UPDATE -----------------

        public async Task<IngredientResponse?> UpdateAsync(int id, UpdateIngredientRequest request)
        {
            var model = await _context.Ingredients.FindAsync(id);
            if (model == null) return null;

            model.Name = request.Name;
            model.Category = request.Category;
            model.UpdatedAt = DateTime.UtcNow;

            // Nutritional
            model.WaterPct = request.WaterPct;
            model.ProteinPct = request.ProteinPct;
            model.CarbsPct = request.CarbsPct;
            model.SugarPct = request.SugarPct;
            model.FiberPct = request.FiberPct;
            model.LactosePct = request.LactosePct;
            model.FatPct = request.FatPct;
            model.FatSaturatedPct = request.FatSaturatedPct;
            model.FatMonounsaturatedPct = request.FatMonounsaturatedPct;
            model.FatTransPct = request.FatTransPct;

            // Tech
            model.AlcoholPct = request.AlcoholPct;
            model.Pod = request.Pod;
            model.Pac = request.Pac;
            model.KcalPer100g = request.KcalPer100g;
            model.SodiumMg = request.SodiumMg;
            model.PotassiumMg = request.PotassiumMg;
            model.CholesterolMg = request.CholesterolMg;

            model.TotalSolidsPct = request.TotalSolidsPct;
            model.NonFatSolidsPct = request.NonFatSolidsPct;
            model.MilkSolidsPct = request.MilkSolidsPct;
            model.OtherSolidsPct = request.OtherSolidsPct;

            // Additive
            model.MaxDoseGL = request.MaxDoseGL;
            model.Usage = request.Usage;
            model.Description = request.Description;

            if (request.Scores is not null)
            {
                model.Scores ??= new AdditiveScores();
                model.Scores.Stabilization = request.Scores.Stabilization;
                model.Scores.Emulsifying = request.Scores.Emulsifying;
                model.Scores.LowPhResistance = request.Scores.LowPhResistance;
                model.Scores.Creaminess = request.Scores.Creaminess;
                model.Scores.Viscosity = request.Scores.Viscosity;
                model.Scores.Body = request.Scores.Body;
                model.Scores.Elasticity = request.Scores.Elasticity;
                model.Scores.Crystallization = request.Scores.Crystallization;
            }
            else
            {
                model.Scores = null;
            }

            model.SetIncompatibleWith(request.IncompatibleWith ?? new List<string>());

            if (request.CompatibleWith is not null)
            {
                var compList = request.CompatibleWith
                    .Select(c => new AdditiveCompatibilityItem
                    {
                        Name = c.Name,
                        Percentage = c.Percentage
                    })
                    .ToList();

                model.SetCompatibleWith(compList);
            }
            else
            {
                model.SetCompatibleWith(new List<AdditiveCompatibilityItem>());
            }

            await _context.SaveChangesAsync();

            return model.ToResponse();
        }

        // ----------------- DELETE -----------------

        public async Task<bool> DeleteAsync(int id)
        {
            var model = await _context.Ingredients.FindAsync(id);
            if (model == null) return false;

            _context.Ingredients.Remove(model);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
