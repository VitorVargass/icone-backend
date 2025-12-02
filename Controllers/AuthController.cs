using BCrypt.Net;
using icone_backend.Data;
using icone_backend.Dtos.Auth;
using icone_backend.Models;
using icone_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

namespace icone_backend.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;
        private readonly TwoFactorService _twoFactorService;
        private readonly UserService _userService;


        public AuthController(AppDbContext context, TokenService tokenService, TwoFactorService twoFactorService,
    UserService userService)
        {
            _context = context;
            _tokenService = tokenService;
            _twoFactorService = twoFactorService;
            _userService = userService;
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 10);
        }

        private bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Register(RegisterAccount request)
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
                    Code = "VALIDATION_ERROR",
                    Message = "Existem erros de validação nos campos.",
                    Details = string.Join(" | ", errors.SelectMany(e => e.Errors)),
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            try
            {

                var email = request.Email?.Trim().ToLowerInvariant();
                var documentCompany = request.DocumentCompany?.Trim();
                var fantasyName = request.FantasyName?.Trim();
                var corporateName = request.CorporateName?.Trim();

                if (!string.IsNullOrEmpty(email) && await _context.Users.AnyAsync(u => u.Email == email))
                    return BadRequest(new Error
                    {
                        Code = "DUPLICATE_EMAIL",
                        Message = "E-mail já cadastrado.",
                        Field = "email",
                        TraceId = HttpContext.TraceIdentifier
                    });

                if (!string.IsNullOrEmpty(documentCompany) && await _context.Companies.AnyAsync(c => c.DocumentCompany == documentCompany))
                    return BadRequest(new Error
                    {
                        Code = "DUPLICATE_DOCUMENT_COMPANY",
                        Message = "Documento da empresa já cadastrado.",
                        Field = "documento_empresa",
                        TraceId = HttpContext.TraceIdentifier
                    });

                if (!string.IsNullOrEmpty(fantasyName) && await _context.Companies.AnyAsync(c => c.FantasyName == fantasyName))
                    return BadRequest(new Error
                    {
                        Code = "DUPLICATE_FANTASY_NAME",
                        Message = "Nome Fantasia já cadastrado.",
                        Field = "nome_fantasia",
                        TraceId = HttpContext.TraceIdentifier
                    });

                if (!string.IsNullOrEmpty(corporateName) && await _context.Companies.AnyAsync(c => c.CorporateName == corporateName))
                    return BadRequest(new Error
                    {
                        Code = "DUPLICATE_CORPORATE_NAME",
                        Message = "Nome Corporativo já cadastrado.",
                        Field = "corporate_name",
                        TraceId = HttpContext.TraceIdentifier
                    });

                // valida AddressDto (ModelState já cobre, mas garantimos null-safety)
                var addr = request.Address ?? throw new ArgumentException("Address is required");

                // cria company e user na mesma transação
                await using var tx = await _context.Database.BeginTransactionAsync();

                var company = new CompaniesModel
                {
                    FantasyName = fantasyName ?? string.Empty,
                    CorporateName = corporateName ?? string.Empty,
                    DocumentCompany = documentCompany ?? string.Empty,
                    Phone = request.Phone?.Trim() ?? string.Empty,
                    Website = request.Website ?? string.Empty,
                    Plan = "free", // default — ajuste conforme regras do sistema
                    CountryCode = addr.CountryCode.Trim(),
                    PostalCode = addr.PostalCode.Trim(),
                    StateRegion = addr.StateRegion.Trim(),
                    City = addr.City.Trim(),
                    Line1 = addr.Line1.Trim(),
                    Line2 = addr.Line2?.Trim(),
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                _context.Companies.Add(company);
                await _context.SaveChangesAsync();

                var user = new UserModel
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = email ?? string.Empty,
                    PasswordHash = HashPassword(request.Password),
                    Role = "admin",
                    IsActive = true,
                    CompanyId = company.Id,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                await tx.CommitAsync();

                var token = _tokenService.GenerateToken(user);
                return Ok(new
                {
                    message = "Usuário e empresa criados. Prossiga para próxima etapa.",
                    token
                });

            }
            catch (Exception ex)
            {
                Console.WriteLine("ERRO EM RegisterUser:");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                return StatusCode(500, new { error = ex.Message });
            }
            ;
        }

        // --------------------------------------------------------------------------
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _context.Set<UserModel>()
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
                return Unauthorized(new Error
                {
                    Code = "INVALID_CREDENTIALS",
                    Message = "E-mail ou senha inválidos.",
                    TraceId = HttpContext.TraceIdentifier
                });

            var twoFactorToken = await _twoFactorService.GenerateAndSendCodeAsync(user.Id, user.Email);

            return Ok(new
            {
                message = "Código de verificação enviado para seu e-mail.",
                twoFactorRequired = true,
                twoFactorToken
            });
        }


        [HttpPost("verify-2fa")]
        public async Task<IActionResult> VerifyTwoFactor([FromBody] VerifyTwoFactorRequest request)
        {
            var result = await _twoFactorService.ValidateCodeAsync(request.TwoFactorToken, request.Code);
            if (!result.Success)
            {
                return BadRequest(new { code = "INVALID_2FA_CODE" });
            }

            var user = await _userService.FindByIdAsync(result.UserId);
            if (user == null) return Unauthorized();

            var jwt = _tokenService.GenerateToken(user);
            return Ok(new { token = jwt });
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
        {
            var user = await _context.Set<UserModel>().FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return NotFound(new Error
                {
                    Code = "USER_NOT_FOUND",
                    Message = "Usuário não encontrado.",
                    TraceId = HttpContext.TraceIdentifier
                });

            return Ok(new { message = "Password reset instructions sent to your email." });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMe()
        {
            
            var userIdClaim =
                User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new Error
                {
                    Code = "INVALID_TOKEN",
                    Message = "Token inválido ou usuário não identificado.",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

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

            
            return Ok(new
            {
                id = user.Id,
                firstName = user.FirstName,
                lastName = user.LastName,
                email = user.Email,
                role = user.Role,
                companyId = user.CompanyId,
                isActive = user.IsActive,
                company =  user.Company == null ? null : new
                {
                    id = user.Company.Id,
                    fantasyName = user.Company.FantasyName,
                    corporateName = user.Company.CorporateName,
                    plan = user.Company.Plan,
                    isActive = user.Company.IsActive
                }
            });
        }
    }
}
