using icone_backend.Data;
using icone_backend.Dtos.Additives;
using icone_backend.Dtos.Additives.Requests;
using icone_backend.Dtos.Additives.Responses;
using icone_backend.Interface;
using icone_backend.Models;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace icone_backend.Services.Additive
{
    public class AdditiveService : IAdditiveInterface
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdditiveService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
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

        
        public async Task<IEnumerable<AdditiveResponse>> GetAllAsync(Guid? companyId)
        {
            var userId = GetCurrentUserId();

            var query = _context.Additives.AsQueryable();

            query = query.Where(a =>
                a.Scope == AdditiveScope.System || // globais
                (a.Scope == AdditiveScope.Company
                    && companyId.HasValue
                    && a.CompanyId == companyId)     // da empresa ativa
                ||
                (a.Scope == AdditiveScope.User
                    && !companyId.HasValue
                    && a.CreatedByUserId == userId)  // pessoais do autônomo
            );

            var list = await query.AsNoTracking().ToListAsync();
            return list.Select(MapToResponse);
        }

        public async Task<AdditiveResponse?> GetByIdAsync(int id)
        {
            var a = await _context.Additives.FindAsync(id);
            if (a == null) return null;

            return MapToResponse(a);
        }

        public async Task<AdditiveResponse> CreateAsync(AdditiveRequest request, Guid? companyId)
        {
            var userId = GetCurrentUserId();

            var additive = new AdditiveModel
            {
                Name = request.Name,
                Category = request.Category,
                CreatedByUserId = userId,

                MaxDoseGL = request.MaxDoseGL,
                Usage = request.Usage,
                Description = request.Description
            };

            if (request.Scores is not null)
            {
                additive.Scores = new AdditiveScores
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

            if (request.IncompatibleWith is not null)
            {
                additive.SetIncompatibleWith(request.IncompatibleWith);
            }

            if (request.CompatibleWith is not null)
            {
                var list = request.CompatibleWith
                    .Select(c => new AdditiveCompatibilityItem
                    {
                        Name = c.Name,
                        Percentage = c.Percentage
                    })
                    .ToList();

                additive.SetCompatibleWith(list);
            }

            
            if (companyId.HasValue)
            {
                additive.Scope = AdditiveScope.Company;
                additive.CompanyId = companyId;
            }
            else
            {
                additive.Scope = AdditiveScope.User;
                additive.CompanyId = null;
            }

            _context.Additives.Add(additive);
            await _context.SaveChangesAsync();

            return MapToResponse(additive);
        }

        public async Task<AdditiveResponse?> UpdateAsync(int id, AdditiveRequest request)
        {
            var additive = await _context.Additives.FindAsync(id);
            if (additive == null) return null;

            var userId = GetCurrentUserId();
            

            additive.Name = request.Name;
            additive.Category = request.Category;
            additive.MaxDoseGL = request.MaxDoseGL;
            additive.Usage = request.Usage;
            additive.Description = request.Description;
            additive.UpdatedAt = DateTime.UtcNow;

            if (request.Scores is not null)
            {
                additive.Scores ??= new AdditiveScores();
                additive.Scores.Stabilization = request.Scores.Stabilization;
                additive.Scores.Emulsifying = request.Scores.Emulsifying;
                additive.Scores.LowPhResistance = request.Scores.LowPhResistance;
                additive.Scores.Creaminess = request.Scores.Creaminess;
                additive.Scores.Viscosity = request.Scores.Viscosity;
                additive.Scores.Body = request.Scores.Body;
                additive.Scores.Elasticity = request.Scores.Elasticity;
                additive.Scores.Crystallization = request.Scores.Crystallization;
            }
            else
            {
                additive.Scores = null;
            }

            additive.SetIncompatibleWith(request.IncompatibleWith ?? new List<string>());

            if (request.CompatibleWith is not null)
            {
                var list = request.CompatibleWith
                    .Select(c => new AdditiveCompatibilityItem
                    {
                        Name = c.Name,
                        Percentage = c.Percentage
                    })
                    .ToList();

                additive.SetCompatibleWith(list);
            }
            else
            {
                additive.SetCompatibleWith(new List<AdditiveCompatibilityItem>());
            }

            await _context.SaveChangesAsync();

            return MapToResponse(additive);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var additive = await _context.Additives.FindAsync(id);
            if (additive == null) return false;

            _context.Additives.Remove(additive);
            await _context.SaveChangesAsync();
            return true;
        }

        private static AdditiveResponse MapToResponse(AdditiveModel additive)
        {
            var incompatible = additive.GetIncompatibleWith();
            var compItems = additive.GetCompatibleWith();

            return new AdditiveResponse
            {
                Id = additive.Id,
                Name = additive.Name,
                Category = additive.Category,
                Scope = (int)additive.Scope,
                IsReadOnly = additive.IsReadOnly,
                MaxDoseGL = additive.MaxDoseGL,
                Usage = additive.Usage,
                Description = additive.Description,

                Scores = additive.Scores is null
                    ? null
                    : new AdditiveScoresDto
                    {
                        Stabilization = additive.Scores.Stabilization,
                        Emulsifying = additive.Scores.Emulsifying,
                        LowPhResistance = additive.Scores.LowPhResistance,
                        Creaminess = additive.Scores.Creaminess,
                        Viscosity = additive.Scores.Viscosity,
                        Body = additive.Scores.Body,
                        Elasticity = additive.Scores.Elasticity,
                        Crystallization = additive.Scores.Crystallization
                    },

                IncompatibleWith = incompatible,
                CompatibleWith = compItems
                    .Select(c => new AdditiveCompatibilityDto
                    {
                        Name = c.Name,
                        Percentage = c.Percentage
                    })
                    .ToList(),

                CompanyId = additive.CompanyId,
                UserId = additive.CreatedByUserId
            };
        }
    }
}
