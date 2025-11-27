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
                var token = _tokenService.GenerateToken(user);
                return Ok(new
                {
                    message = "Usuário criado. Prossiga para proxima etapa.", token

                });
            } catch (Exception ex) {

                Console.WriteLine("ERRO EM RegisterUser:");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                return StatusCode(500, new { error = ex.Message });
            }
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

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMe()
        {
            // 1) Pega o ID do usuário a partir do token (claim "sub")
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

            // 3) Monta o objeto de resposta (só o que o front precisa)
            return Ok(new
            {
                id = user.Id,
                firstName = user.FirstName,
                lastName = user.LastName,
                email = user.Email,
                role = user.Role,
                companyId = user.CompanyId,
                onboardingStep = user.OnboardingStep.ToString().ToLowerInvariant(),
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