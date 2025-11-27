using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using icone_backend.Data;
using icone_backend.Dtos.Auth;
using icone_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace icone_backend.Controllers
{
    [ApiController]
    [Route("Onboarding")]
    [Authorize] 
    public class OnboardingController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OnboardingController(AppDbContext context)
        {
            _context = context;
        }

        
        private long GetCurrentUserId()
        {
            var userIdClaim =
                User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                throw new Exception("Invalid token: user id not found.");
            }

            return userId;
        }

        // GET /Onboarding/company
        [HttpGet("GetCompany")]
        public async Task<IActionResult> GetCompany()
        {
            var userId = GetCurrentUserId();

            var user = await _context.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound(new Error
                {
                    Code = "USER_NOT_FOUND",
                    Message = "Usuário não encontrado.",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            if (user.Company == null)
            {
                return NotFound(new Error
                {
                    Code = "COMPANY_NOT_FOUND",
                    Message = "Usuário ainda não possui empresa vinculada.",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            var company = user.Company;

            return Ok(new
            {
                document = company.Document,
                fantasyName = company.FantasyName,
                corporateName = company.CorporateName,
                phone = company.Phone,
                website = company.Website,
                address = new
                {
                    countryCode = company.CountryCode,
                    postalCode = company.PostalCode,
                    stateRegion = company.StateRegion,
                    city = company.City,
                    line1 = company.Line1,
                    line2 = company.Line2
                },
                plan = company.Plan,
                isActive = company.IsActive,
                createdAt = company.CreatedAt
            });
        }



        // PUT /Onboarding/company
        [HttpPut("company")]
        public async Task<IActionResult> RegisterOrUpdateCompany(RegisterCompanyStepRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .Select(x => new
                    {
                        Field = x.Key,
                        Errors = x.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    });

                return BadRequest(new Error
                {
                    Code = "VALIDATION_ERROR_COMPANY",
                    Message = "Existem erros de validação nos campos.",
                    Details = string.Join(" | ", errors.SelectMany(e => e.Errors)),
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            // 1) Pega o usuário logado pelo token (não usa mais request.UserId)
            var userId = GetCurrentUserId();

            var user = await _context.Users
                .Include(u => u.Company) // navigation property Company (precisa existir no UserModel)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound(new Error
                {
                    Code = "USER_NOT_FOUND",
                    Message = "Usuário não encontrado.",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            // 2) Valida duplicidades (ignorando a empresa atual do usuário, se tiver)
            if (await _context.Companies.AnyAsync(c =>
                    c.Document == request.Document &&
                    c.Id != (user.CompanyId ?? 0)))
            {
                return BadRequest(new Error
                {
                    Code = "DUPLICATE_DOCUMENT_COMPANY",
                    Message = "Documento da empresa já cadastrado.",
                    Field = "documento_empresa",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            if (await _context.Companies.AnyAsync(c =>
                    c.FantasyName == request.FantasyName &&
                    c.Id != (user.CompanyId ?? 0)))
            {
                return BadRequest(new Error
                {
                    Code = "DUPLICATE_FANTASY_NAME",
                    Message = "Nome Fantasia já cadastrado.",
                    Field = "nome_fantasia",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            if (await _context.Companies.AnyAsync(c =>
                    c.CorporateName == request.CorporateName &&
                    c.Id != (user.CompanyId ?? 0)))
            {
                return BadRequest(new Error
                {
                    Code = "DUPLICATE_CORPORATE_NAME",
                    Message = "Nome Corporativo já cadastrado.",
                    Field = "corporate_name",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            var address = request.Address;

            CompaniesModel company;

            // 3) Se o usuário já tem empresa, atualiza; senão, cria
            if (user.Company != null)
            {
                company = user.Company;

                company.Document = request.Document;
                company.FantasyName = request.FantasyName;
                company.CorporateName = request.CorporateName;
                company.Phone = request.Phone;
                company.Website = request.Website;

                company.CountryCode = address.CountryCode.ToUpperInvariant();
                company.PostalCode = address.PostalCode;
                company.StateRegion = address.StateRegion;
                company.City = address.City;
                company.Line1 = address.Line1;
                company.Line2 = address.Line2;

                // mantém Plan, IsActive, CreatedAt
                _context.Companies.Update(company);
            }
            else
            {
                company = new CompaniesModel
                {
                    Document = request.Document,
                    FantasyName = request.FantasyName,
                    CorporateName = request.CorporateName,
                    Phone = request.Phone,
                    Website = request.Website,

                    CountryCode = address.CountryCode.ToUpperInvariant(),
                    PostalCode = address.PostalCode,
                    StateRegion = address.StateRegion,
                    City = address.City,
                    Line1 = address.Line1,
                    Line2 = address.Line2,

                    Plan = "0",
                    IsActive = false,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                _context.Companies.Add(company);
                await _context.SaveChangesAsync(); // pra gerar o Id
                user.CompanyId = company.Id;
            }

            // 4) Atualiza o step do onboarding do usuário
            user.OnboardingStep = OnboardingSteps.Company;
            _context.Users.Update(user);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Empresa vinculada/atualizada com sucesso.",
                companyId = company.Id
            });
        
        }
        [HttpPut("plan")]
        public async Task<IActionResult> UpdatePlan([FromBody] UpdatePlanRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PlanCode))
                return BadRequest(new { success = false, message = "O campo 'planCode' é obrigatório." });

            var userId = GetCurrentUserId();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound(new { success = false, message = "Usuário não encontrado." });

            if (user.CompanyId == null)
                return BadRequest(new { success = false, message = "Usuário não possui empresa vinculada." });

            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == user.CompanyId);
            if (company == null)
                return NotFound(new { success = false, message = "Empresa não encontrada." });

            company.Plan = request.PlanCode;
            user.OnboardingStep = OnboardingSteps.Plan;

            _context.Update(company);
            _context.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        [HttpPost("complete")]
        public async Task<IActionResult> CompleteOnboarding([FromBody] CompleteOnboardingRequest request)
        {
            string? planCode = request.PlanCode; 
            var userId = GetCurrentUserId();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return NotFound(new { success = false, message = "Usuário não encontrado." });

            if (user.CompanyId == null)
                return BadRequest(new { success = false, message = "Usuário não possui empresa vinculada." });

            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == user.CompanyId);
            if (company == null)
                return NotFound(new { success = false, message = "Empresa não encontrada." });

            if (!string.IsNullOrEmpty(planCode))
                company.Plan = planCode;

            
            user.IsActive = true;
            user.OnboardingStep = OnboardingSteps.Completed;

            _context.Update(user);
            _context.Update(company);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetOnboardingStatus()
        {
            var userId = GetCurrentUserId();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return NotFound(new { message = "Usuário não encontrado." });

            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == user.CompanyId);

            return Ok(new
            {
                onboardingStep = user.OnboardingStep,
                isActive = user.IsActive,
                companyId = user.CompanyId,
                planCode = company?.Plan
            });
        }
    }
}
