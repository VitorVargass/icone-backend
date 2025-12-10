using icone_backend.Data;
using icone_backend.Dtos.Additives.Requests;
using icone_backend.Dtos.Neutral;
using icone_backend.Dtos.Neutral.Requests;
using icone_backend.Dtos.Neutral.Responses;
using icone_backend.Interface;
using icone_backend.Models;
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

           
            var neutrals = await _context.Neutrals
                .AsNoTracking()
                .ToListAsync(ct);

            
            neutrals = neutrals
                .Where(n =>
                    n.Scope == NeutralScope.System || 
                    (n.Scope == NeutralScope.Company
                        && companyId.HasValue
                        && n.CompanyId == companyId)     
                    ||
                    (n.Scope == NeutralScope.User
                        && !companyId.HasValue
                        && n.CreatedByUserId == userId)  
                )
                .ToList();

            
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

        public async Task<NeutralResponse?> UpdateAsync(int id, CreateNeutralRequest request, CancellationToken ct)
        {
            var neutral = await _context.Neutrals
                .FirstOrDefaultAsync(n => n.Id == id, ct);

            if (neutral == null)
                return null;

            // Reaproveita a mesma lógica de montar entidade e componentes
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

            neutral.Name = request.Name;
            neutral.GelatoType = request.GelatoType;
            neutral.Method = request.Method;
            neutral.RecommendedDoseGPerKg = request.RecommendedDoseGPerKg;
            neutral.UpdatedAt = DateTime.UtcNow;

            var resolvedComponents = new List<NeutralComponentResolved>();
            var jsonItems = new List<NeutralComponentItem>();

            foreach (var c in request.Components)
            {
                var additive = additives.First(a => a.Id == c.AdditiveId);
                var qty = NormalizeQuantity(c.QuantityPerLiter);

                resolvedComponents.Add(new NeutralComponentResolved
                {
                    Additive = additive,
                    QuantityPerLiter = qty
                });

                jsonItems.Add(new NeutralComponentItem
                {
                    AdditiveId = additive.Id,
                    QuantityPerLiter = qty
                });
            }

            // atualiza o JSON dos componentes
            neutral.SetComponents(jsonItems);

            // valida de novo
            var messages = ValidateNeutral(neutral, resolvedComponents);

            if (messages.Errors.Any())
                throw new InvalidOperationException("Neutral has validation errors.");

            await _context.SaveChangesAsync(ct);

            return MapToResponse(neutral, resolvedComponents, messages);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct)
        {
            var neutral = await _context.Neutrals
                .FirstOrDefaultAsync(n => n.Id == id, ct);

            if (neutral == null)
                return false;

            _context.Neutrals.Remove(neutral);
            await _context.SaveChangesAsync(ct);

            return true;
        }

        public async Task<AdditiveScoresDto> AnalyzeDraftAsync( CreateNeutralRequest request, CancellationToken ct)
        {
            // Reaproveita a lógica existente para montar o neutro e resolver os aditivos
            var (neutral, resolvedComponents) = await BuildNeutralAggregateAsync(request, ct);

            // Se quiser, ainda pode validar o neutro (dose total, etc.),
            // mas aqui a gente só precisa das características finais
            var total = resolvedComponents.Sum(c => c.QuantityPerLiter);

            var result = new AdditiveScoresDto();

            // se não tem dose total ou ninguém tem Scores, devolve tudo 0
            if (total <= 0 || !resolvedComponents.Any(c => c.Additive.Scores != null))
            {
                return result;
            }

            double WeightedAverage(Func<AdditiveScores, double> selector)
            {
                var numerador = resolvedComponents
                    .Where(c => c.Additive.Scores != null)
                    .Sum(c => selector(c.Additive.Scores!) * c.QuantityPerLiter);

                return numerador / total;
            }

            result.Stabilization = WeightedAverage(s => s.Stabilization);
            result.Emulsifying = WeightedAverage(s => s.Emulsifying);
            result.LowPhResistance = WeightedAverage(s => s.LowPhResistance);
            result.Creaminess = WeightedAverage(s => s.Creaminess);
            result.Viscosity = WeightedAverage(s => s.Viscosity);
            result.Body = WeightedAverage(s => s.Body);
            result.Elasticity = WeightedAverage(s => s.Elasticity);
            result.Crystallization = WeightedAverage(s => s.Crystallization);

            return result;
        }

        // ----------------- helpers -----------------
        private static double NormalizeQuantity(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return 0d;

            var v = raw.Trim();

            // aceita vírgula como decimal (pt-BR)
            v = v.Replace(',', '.');

            // separa parte inteira e decimal
            var parts = v.Split('.', 2);
            var intPart = parts[0];

            // remove zeros à esquerda da parte inteira
            intPart = intPart.TrimStart('0');
            if (intPart == string.Empty)
                intPart = "0";

            v = parts.Length == 2 ? $"{intPart}.{parts[1]}" : intPart;

            if (!double.TryParse(
                    v,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var result))
            {
                throw new InvalidOperationException($"Invalid quantity: '{raw}'");
            }

            return result;
        }

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
                var qty = NormalizeQuantity(c.QuantityPerLiter);

                resolvedComponents.Add(new NeutralComponentResolved
                {
                    Additive = additive,
                    QuantityPerLiter = qty
                });

                jsonItems.Add(new NeutralComponentItem
                {
                    AdditiveId = additive.Id,
                    QuantityPerLiter = qty
                });
            }

           
            neutral.SetComponents(jsonItems);

            return (neutral, resolvedComponents);
        }

        private NeutralMessagesDto ValidateNeutral(Neutral neutral, List<NeutralComponentResolved> components)
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
                Scope = neutral.Scope,
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
