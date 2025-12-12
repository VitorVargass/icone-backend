using icone_backend.Data;
using icone_backend.Dtos.Ingredient.Responses;
using icone_backend.Dtos.Neutral;
using icone_backend.Dtos.Neutral.Requests;
using icone_backend.Dtos.Neutral.Responses;
using icone_backend.Interface;
using icone_backend.Models;
using icone_backend.Utils;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace icone_backend.Services.NeutralService
{
    public class NeutralService : INeutral
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly NeutralValidator _validator;

        public NeutralService(
            AppDbContext context,
            IHttpContextAccessor httpContextAccessor,
            NeutralValidator validator)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _validator = validator;
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

        public async Task<IReadOnlyList<NeutralResponse>> GetAllAsync(Guid? companyId, CancellationToken ct)
        {
            var userId = GetCurrentUserId();

            var neutrals = await _context.Neutrals.AsNoTracking().ToListAsync(ct);

            neutrals = neutrals
               .Where(n =>
                    n.Scope == NeutralScope.System ||
                    (n.Scope == NeutralScope.Company &&
                        companyId.HasValue &&
                        n.CompanyId == companyId) ||
                    (n.Scope == NeutralScope.User &&
                        !companyId.HasValue &&
                        n.CreatedByUserId == userId)
                )
                .ToList();

            var allItems = neutrals.SelectMany(n => n.GetComponents()).ToList();

            var ingredientIds = allItems.Select(c => c.IngredientId).Distinct().ToList();

          
            var ingredients = await _context.Ingredients.Where(i => ingredientIds.Contains(i.Id)).ToListAsync(ct);

            var ingredientDict = ingredients.ToDictionary(i => i.Id);

            var responses = new List<NeutralResponse>();

            foreach (var neutral in neutrals)
            {
                var items = neutral.GetComponents();

                var resolvedComponents = items.Where(ci => ingredientDict.ContainsKey(ci.IngredientId))
                    .Select(ci => (
                        ingredient: ingredientDict[ci.IngredientId],
                        quantityPerLiter: ci.QuantityPerLiter
                    ))
                    .ToList();

                var resp = neutral.ToResponse(resolvedComponents, new NeutralMessagesDto());
                responses.Add(resp);
            }

            return responses;
        }

        public async Task<NeutralResponse?> GetByIdAsync(int id, CancellationToken ct)
        {
            var neutral = await _context.Neutrals.FirstOrDefaultAsync(n => n.Id == id, ct);

            if (neutral == null) return null;

            var items = neutral.GetComponents();

            var ingredientIds = items.Select(i => i.IngredientId).Distinct().ToList();

            var ingredients = await _context.Ingredients.Where(i => ingredientIds.Contains(i.Id)).ToListAsync(ct);

            var ingredientDict = ingredients.ToDictionary(i => i.Id);

            var resolvedComponents = items.Where(ci => ingredientDict.ContainsKey(ci.IngredientId))
                .Select(ci => (
                    ingredient: ingredientDict[ci.IngredientId],
                    quantityPerLiter: ci.QuantityPerLiter
                ))
                .ToList();

            return neutral.ToResponse(resolvedComponents, new NeutralMessagesDto());
        }

        // ----------------- CREATE -----------------

        public async Task<NeutralResponse> CreateAsync(CreateNeutralRequest request, Guid? companyId, CancellationToken ct)
        {
            var userId = GetCurrentUserId();

            var ingredientIds = request.Components.Select(c => c.IngredientId).Distinct().ToList();

            var ingredients = await _context.Ingredients.Where(i => ingredientIds.Contains(i.Id)).ToListAsync(ct);

            var foundIds = ingredients.Select(i => i.Id).ToHashSet();

            var missingIds = ingredientIds
                .Where(id => !foundIds.Contains(id))
                .ToList();

            if (missingIds.Count > 0)
            {
                throw new InvalidOperationException(
                    $"One or more ingredients not found. Missing: {string.Join(", ", missingIds)}");
            }

            if (ingredients.Count != ingredientIds.Count)
                throw new InvalidOperationException("One or more ingredients not found.");

            var neutral = new Neutral
            {
                Name = request.Name,
                GelatoType = request.GelatoType,
                Method = request.Method,
                RecommendedDoseGPerKg = request.RecommendedDoseGPerKg
            };

            var resolvedComponents = new List<(IngredientModel ingredient, double quantityPerLiter)>();
            var jsonItems = new List<NeutralComponentItem>();

            foreach (var c in request.Components)
            {
                var ingredient = ingredients.First(i => i.Id == c.IngredientId);

                var qtyDouble = double.TryParse(c.QuantityPerLiter, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedQty) ? parsedQty
                        : throw new InvalidOperationException($"Quantidade inválida: {c.QuantityPerLiter}");

                resolvedComponents.Add((ingredient, qtyDouble));

                jsonItems.Add(new NeutralComponentItem
                {
                    IngredientId = ingredient.Id,
                    QuantityPerLiter = qtyDouble
                });
            }

            neutral.SetComponents(jsonItems);

            neutral.CreatedByUserId = userId;
            neutral.CreatedAt = DateTime.UtcNow;

            if (companyId.HasValue)
            {
                neutral.Scope = NeutralScope.Company;
                neutral.CompanyId = companyId;
            }
            else
            {
                neutral.Scope = NeutralScope.User;
                neutral.CompanyId = null;
            }

            var messages = _validator.Validate(neutral, resolvedComponents);

            if (messages.Errors.Any())
                throw new InvalidOperationException("Neutral has validation errors.");

            _context.Neutrals.Add(neutral);
            await _context.SaveChangesAsync(ct);

            return neutral.ToResponse(resolvedComponents, messages);
        }

        // ----------------- UPDATE -----------------

        public async Task<NeutralResponse?> UpdateAsync(int id, CreateNeutralRequest request, CancellationToken ct)
        {
            var neutral = await _context.Neutrals.FirstOrDefaultAsync(n => n.Id == id, ct);

            if (neutral == null)
                return null;

            var ingredientIds = request.Components.Select(c => c.IngredientId).Distinct().ToList();

            var ingredients = await _context.Ingredients.Where(i => ingredientIds.Contains(i.Id)).ToListAsync(ct);

            var foundIds = ingredients.Select(i => i.Id).ToHashSet();

            var missingIds = ingredientIds
                .Where(id => !foundIds.Contains(id))
                .ToList();

            if (missingIds.Count > 0)
            {
                throw new InvalidOperationException(
                    $"One or more ingredients not found. Missing: {string.Join(", ", missingIds)}");
            }


            if (ingredients.Count != ingredientIds.Count)
                throw new InvalidOperationException("One or more ingredients not found.");

            neutral.Name = request.Name;
            neutral.GelatoType = request.GelatoType;
            neutral.Method = request.Method;
            neutral.RecommendedDoseGPerKg = request.RecommendedDoseGPerKg;
            neutral.UpdatedAt = DateTime.UtcNow;

            var resolvedComponents = new List<(IngredientModel ingredient, double quantityPerLiter)>();
            var jsonItems = new List<NeutralComponentItem>();

            foreach (var c in request.Components)
            {
                var ingredient = ingredients.First(i => i.Id == c.IngredientId);

                var qtyDouble =
                    double.TryParse(c.QuantityPerLiter, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedQty) ? parsedQty
                        : throw new InvalidOperationException($"Quantidade inválida: {c.QuantityPerLiter}");

                resolvedComponents.Add((ingredient, qtyDouble));

                jsonItems.Add(new NeutralComponentItem
                {
                    IngredientId = ingredient.Id,
                    QuantityPerLiter = qtyDouble
                });
            }

            neutral.SetComponents(jsonItems);

            var messages = _validator.Validate(neutral, resolvedComponents);

            if (messages.Errors.Any())
                throw new InvalidOperationException("Neutral has validation errors.");

            await _context.SaveChangesAsync(ct);

            return neutral.ToResponse(resolvedComponents, messages);
        }

        // ----------------- DELETE -----------------

        public async Task<bool> DeleteAsync(int id, CancellationToken ct)
        {
            var neutral = await _context.Neutrals.FirstOrDefaultAsync(n => n.Id == id, ct);

            if (neutral == null)
                return false;

            _context.Neutrals.Remove(neutral);
            await _context.SaveChangesAsync(ct);

            return true;
        }
    }
}
