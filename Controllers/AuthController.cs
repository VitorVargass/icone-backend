using icone_backend.Data;
using icone_backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Security.Cryptography;
using icone_backend.Dtos.Auth;
using BCrypt.Net;
using icone_backend.Services;



namespace icone_backend.Controllers
{
    [ApiController]
    [Route("Auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;

        public AuthController(AppDbContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 10);
        }

        private bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        [HttpPost("register-user")]
        public async Task<IActionResult> RegisterUser(RegisterUserStepRequest request)
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
                if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                    return BadRequest(new Error
                    {
                        Code = "DUPLICATE_EMAIL",
                        Message = "E-mail já cadastrado.",
                        Field = "email",
                        TraceId = HttpContext.TraceIdentifier
                    });

                if (await _context.Users.AnyAsync(u => u.Document == request.Document))
                    return BadRequest(new Error
                    {
                        Code = "DUPLICATE_DOCUMENT",
                        Message = "Documento já cadastrado.",
                        Field = "documento",
                        TraceId = HttpContext.TraceIdentifier
                    });

                var user = new UserModel
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    PasswordHash = HashPassword(request.Password),
                    Role = "manager",
                    Document = request.Document,
                    IsActive = false,
                    OnboardingStep = OnboardingSteps.Contact,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Usuário criado. Prossiga para a etapa de empresa.",
                    userId = user.Id
                });
            } catch (Exception ex) {

                Console.WriteLine("ERRO EM RegisterUser:");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("register-company")]
        public async Task<IActionResult> RegisterCompany(RegisterCompanyStepRequest request)
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

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId);
            if (user == null)
                return NotFound(new Error
                {
                    Code = "USER_NOT_FOUND",
                    Message = "Usuário não encontrado.",
                    TraceId = HttpContext.TraceIdentifier
                });

            if (await _context.Companies.AnyAsync(u => u.Document == request.Document))
                return BadRequest(new Error
                {
                    Code = "DUPLICATE_DOCUMENT_COMPANY",
                    Message = "Documento da empresa já cadastrado.",
                    Field = "documento_empresa",
                    TraceId = HttpContext.TraceIdentifier
                });
            if (await _context.Companies.AnyAsync(u => u.FantasyName == request.FantasyName))
                return BadRequest(new Error
                {
                    Code = "DUPLICATE_FANTASY_NAME",
                    Message = "Nome Fantasia já cadastrado.",
                    Field = "nome_fantasia",
                    TraceId = HttpContext.TraceIdentifier
                });
            if (await _context.Companies.AnyAsync(u => u.CorporateName == request.CorporateName))
                return BadRequest(new Error
                {
                    Code = "DUPLICATE_CORPORATE_NAME",
                    Message = "Nome Corporativo já cadastrado.",
                    Field = "corporate_name",
                    TraceId = HttpContext.TraceIdentifier
                });

            var address = request.Address;

            var company = new CompaniesModel
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
            await _context.SaveChangesAsync();

            user.CompanyId = company.Id;
            user.OnboardingStep = OnboardingSteps.Company;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Empresa vinculada com sucesso.",
                companyId = company.Id
            });
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

            var token = _tokenService.GenerateToken(user);

           return Ok(new { message = "Login successful!", token});
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
    }
}