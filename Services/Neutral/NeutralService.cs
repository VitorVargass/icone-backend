using icone_backend.Data;
using icone_backend.Dtos.Neutral;
using icone_backend.Dtos.Neutral.Requests;
using icone_backend.Dtos.Neutral.Responses;
using icone_backend.Interface;
using icone_backend.Models;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace icone_backend.Services.NeutralService
{
    public class NeutralService : INeutral
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NeutralService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private long GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null)
                throw new UnauthorizedAccessException("User not authenticated.");

            var idStr =
                user.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(idStr) || !long.TryParse(idStr, out var userId))
                throw new UnauthorizedAccessException("Invalid user id in token.");

            return userId;
        }

        public async Task<IReadOnlyList<NeutralResponse>> GetAllAsync(Guid? companyId, CancellationToken ct)
        {
            var userId = GetCurrentUserId();

            // carrega todos neutros
            var neutrals = await _context.Neutrals
                .AsNoTracking()
                .ToListAsync(ct);

            // 🔹 aplica a MESMA lógica de escopo de Ingredient/Additive
            neutrals = neutrals
                .Where(n =>
                    n.Scope == NeutralScope.System || // globais
                    (n.Scope == NeutralScope.Company
                        && companyId.HasValue
                        && n.CompanyId == companyId)     // da empresa ativa
                    ||
                    (n.Scope == NeutralScope.User
                        && !companyId.HasValue
                        && n.CreatedByUserId == userId)  // pessoais do autônomo
                )
                .ToList();

            // resolve todos AdditiveIds usados
            var allItems = neutrals
                .SelectMany(n => n.GetComponents())
                .ToList();

            var additiveIds = allItems
                .Select(c => c.AdditiveId)
                .Distinct()
                .ToList();

            var additives = await _context.Additives
                .Where(a => additiveIds.Contains(a.Id))
                .ToListAsync(ct);

            var additivesDict = additives.ToDictionary(a => a.Id);

            var responses = new List<NeutralResponse>();

            foreach (var neutral in neutrals)
            {
                var items = neutral.GetComponents();

                var resolvedComponents = items
                    .Where(ci => additivesDict.ContainsKey(ci.AdditiveId))
                    .Select(ci => new NeutralComponentResolved
                    {
                        Additive = additivesDict[ci.AdditiveId],
                        QuantityPerLiter = ci.QuantityPerLiter
                    })
                    .ToList();

                var resp = MapToResponse(neutral, resolvedComponents, new NeutralMessagesDto());
                responses.Add(resp);
            }

            return responses;
        }


        public async Task<NeutralResponse?> GetByIdAsync(int id, CancellationToken ct)
        {
            var neutral = await _context.Neutrals
                .FirstOrDefaultAsync(n => n.Id == id, ct);

            if (neutral == null) return null;

            var items = neutral.GetComponents();
            var additiveIds = items.Select(i => i.AdditiveId).Distinct().ToList();

            var additives = await _context.Additives
                .Where(a => additiveIds.Contains(a.Id))
                .ToListAsync(ct);

            var additivesDict = additives.ToDictionary(a => a.Id);

            var resolvedComponents = items
                .Where(ci => additivesDict.ContainsKey(ci.AdditiveId))
                .Select(ci => new NeutralComponentResolved
                {
                    Additive = additivesDict[ci.AdditiveId],
                    QuantityPerLiter = ci.QuantityPerLiter
                })
                .ToList();

            return MapToResponse(neutral, resolvedComponents, new NeutralMessagesDto());
        }

        public async Task<NeutralResponse> PreviewAsync(CreateNeutralRequest request, CancellationToken ct)
        {
            var (neutral, resolvedComponents) = await BuildNeutralAggregateAsync(request, ct);
            var messages = ValidateNeutral(neutral, resolvedComponents);
            return MapToResponse(neutral, resolvedComponents, messages);
        }

        public async Task<NeutralResponse> CreateAsync( CreateNeutralRequest request, Guid? companyId, CancellationToken ct)
        {
            var userId = GetCurrentUserId();

            var (neutral, resolvedComponents) = await BuildNeutralAggregateAsync(request, ct);

            
            neutral.CreatedByUserId = userId;

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

            var messages = ValidateNeutral(neutral, resolvedComponents);

            if (messages.Errors.Any())
                throw new InvalidOperationException("Neutral has validation errors.");

            _context.Neutrals.Add(neutral);
            await _context.SaveChangesAsync(ct);

            return MapToResponse(neutral, resolvedComponents, messages);
        }


        // ----------------- helpers -----------------


        private async Task<(Neutral neutral, List<NeutralComponentResolved> components)>
            BuildNeutralAggregateAsync(CreateNeutralRequest request, CancellationToken ct)
        {
            var additiveIds = request.Components
                .Select(c => c.AdditiveId)
                .Distinct()
                .ToList();

            var additives = await _context.Additives
                .Where(a => additiveIds.Contains(a.Id))
                .ToListAsync(ct);

            if (additives.Count != additiveIds.Count)
            {
                throw new InvalidOperationException("One or more additives not found.");
            }

            var neutral = new Neutral
            {
                Name = request.Name,
                GelatoType = request.GelatoType,
                Method = request.Method,
                RecommendedDoseGPerKg = request.RecommendedDoseGPerKg
            };

            var resolvedComponents = new List<NeutralComponentResolved>();
            var jsonItems = new List<NeutralComponentItem>();

            foreach (var c in request.Components)
            {
                var additive = additives.First(a => a.Id == c.AdditiveId);

                resolvedComponents.Add(new NeutralComponentResolved
                {
                    Additive = additive,
                    QuantityPerLiter = c.QuantityPerLiter
                });

                jsonItems.Add(new NeutralComponentItem
                {
                    AdditiveId = additive.Id,
                    QuantityPerLiter = c.QuantityPerLiter
                });
            }

           
            neutral.SetComponents(jsonItems);

            return (neutral, resolvedComponents);
        }

        private NeutralMessagesDto ValidateNeutral(
            Neutral neutral,
            List<NeutralComponentResolved> components)
        {
            var messages = new NeutralMessagesDto();

            
            var total = components.Sum(c => c.QuantityPerLiter);
            neutral.TotalDosagePerLiter = total;

            if (total < 5.0)
                messages.Warnings.Add($"Total = {total:0.###} g/L → Recommended 5 g/L.");
            else if (total > 5.0)
                messages.Errors.Add($"Total = {total:0.###} g/L → Maximum is 5 g/L.");

            // Validation MaxDoseGL
            foreach (var comp in components)
            {
                var a = comp.Additive;

                if (a.MaxDoseGL.HasValue && comp.QuantityPerLiter > a.MaxDoseGL.Value)
                {
                    messages.Errors.Add(
                        $"Additive '{a.Name}': {comp.QuantityPerLiter:0.###} g/L exceeds max dose {a.MaxDoseGL:0.###} g/L."
                    );
                }
            }

            
            var method = neutral.Method?.Trim().ToLowerInvariant();
            foreach (var comp in components)
            {
                var a = comp.Additive;

                if (string.IsNullOrWhiteSpace(method)) continue;

                if (method == "hot" && a.Usage == AdditiveUsage.Cold)
                {
                    messages.Warnings.Add(
                        $"Additive '{a.Name}' is marked as Cold but neutral method is Hot."
                    );
                }

                if (method == "cold" && a.Usage == AdditiveUsage.Hot)
                {
                    messages.Warnings.Add(
                        $"Additive '{a.Name}' is marked as Hot but neutral method is Cold."
                    );
                }
            }

            // Find incomatible additives
            for (int i = 0; i < components.Count; i++)
            {
                for (int j = i + 1; j < components.Count; j++)
                {
                    var a = components[i].Additive;
                    var b = components[j].Additive;

                    var aIncompat = a.GetIncompatibleWith();
                    var bIncompat = b.GetIncompatibleWith();

                    if (aIncompat.Any(n => string.Equals(n, b.Name, StringComparison.OrdinalIgnoreCase)) ||
                        bIncompat.Any(n => string.Equals(n, a.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        messages.Errors.Add(
                            $"Additives '{a.Name}' and '{b.Name}' are marked as incompatible."
                        );
                    }
                }
            }

            return messages;
        }

        private static NeutralResponse MapToResponse( Neutral neutral, List<NeutralComponentResolved> components, NeutralMessagesDto messages)
        {
            return new NeutralResponse
            {
                Id = neutral.Id,
                Name = neutral.Name,
                GelatoType = neutral.GelatoType,
                Method = neutral.Method,
                RecommendedDoseGPerKg = neutral.RecommendedDoseGPerKg,
                TotalDosagePerLiter = neutral.TotalDosagePerLiter,
                Components = components.Select(c => new NeutralComponentDto
                {
                    AdditiveId = c.Additive.Id,
                    AdditiveName = c.Additive.Name,
                    QuantityPerLiter = c.QuantityPerLiter
                }).ToList(),
                Messages = messages
            };
        }

        private class NeutralComponentResolved
        {
            public AdditiveModel Additive { get; set; } = default!;
            public double QuantityPerLiter { get; set; }
        }
    }
}
