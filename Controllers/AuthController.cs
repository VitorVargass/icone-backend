using BCrypt.Net;
using icone_backend.Data;
using icone_backend.Dtos.Auth;
using icone_backend.Models;
using icone_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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

        private const string AuthCookieName = "icone_auth";

        public AuthController(
            AppDbContext context,
            TokenService tokenService,
            TwoFactorService twoFactorService,
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

        /// <summary>
        /// Seta o cookie HttpOnly com o JWT
        /// </summary>
        private void SetAuthCookie(string jwt)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,                 
                SameSite = SameSiteMode.None,  
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                Domain = ".icone.academy"   
            };

            Response.Cookies.Append(AuthCookieName, jwt, cookieOptions);
        }

        /// <summary>
        /// Remove o cookie de autenticação
        /// </summary>
        private void ClearAuthCookie()
        {
            Response.Cookies.Delete(AuthCookieName);
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

                if (string.IsNullOrEmpty(email) || !_twoFactorService.IsSignupEmailVerified(email))
                {
                    return BadRequest(new Error
                    {
                        Code = "EMAIL_NOT_VERIFIED",
                        Message = "Você precisa verificar o e-mail antes de continuar.",
                        Field = "email",
                        TraceId = HttpContext.TraceIdentifier
                    });
                }

                if (!string.IsNullOrEmpty(email) && await _context.Users.AnyAsync(u => u.Email == email))
                {
                    return BadRequest(new Error
                    {
                        Code = "DUPLICATE_EMAIL",
                        Message = "E-mail já cadastrado.",
                        Field = "email",
                        TraceId = HttpContext.TraceIdentifier
                    });
                }

                var user = new UserModel
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = email ?? string.Empty,
                    PasswordHash = HashPassword(request.Password),
                    Role = "admin",
                    Plan = request.Plan,
                    IsActive = true,
                    CompanyId = null,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // gera JWT e já seta cookie (usuário logado após signup)
                var token = _tokenService.GenerateToken(user);
                SetAuthCookie(token);

                return Ok(new
                {
                    message = "Usuário criado. Prossiga para próxima etapa.",
                    token // continua mandando no body se você ainda usar no front
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERRO EM RegisterUser:");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Request Email and send Code
        [HttpPost("signup/email-code")]
        public async Task<IActionResult> SendSignupEmailCode([FromBody] SignupEmailCodeRequest request)
        {
            var email = request.Email?.Trim().ToLowerInvariant();

            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new { message = "E-mail é obrigatório." });
            }

            await _twoFactorService.SendSignupEmailCodeAsync(email);

            return Ok(new { message = "Enviamos um código de verificação para o seu e-mail." });
        }

        // Verify Email Signup
        [HttpPost("signup/email-code/verify")]
        public async Task<IActionResult> VerifySignupEmailCode([FromBody] VerifySignupEmailCodeRequest request)
        {
            var email = request.Email?.Trim().ToLowerInvariant();
            var code = request.Code?.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(code))
            {
                return BadRequest(new { message = "E-mail e código são obrigatórios.", valid = false });
            }

            var valid = await _twoFactorService.VerifySignupEmailCodeAsync(email, code);

            if (!valid)
            {
                return Ok(new { valid = false });
            }

            return Ok(new { valid = true });
        }

        // --------------------------------------------------------------------------
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var email = request.Email?.Trim().ToLowerInvariant();

            var user = await _context.Set<UserModel>()
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            {
                return Unauthorized(new Error
                {
                    Code = "INVALID_CREDENTIALS",
                    Message = "E-mail ou senha inválidos.",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            var now = DateTimeOffset.UtcNow;

            user.LastLoginAt = now;
            await _context.SaveChangesAsync();

            // NÃO gera token definitivo aqui: só depois do 2FA
            var twoFactorToken = await _twoFactorService.GenerateAndSendCodeAsync(user.Id, user.Email);

            return Ok(new
            {
                message = "Código de verificação enviado para seu e-mail.",
                requires2FA = true,
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

            var now = DateTimeOffset.UtcNow;

            user.LastTwoFactorVerifiedAt = now;
            user.LastLoginAt = now;
            await _context.SaveChangesAsync();

            var jwt = _tokenService.GenerateToken(user);

            // Aqui é o "login final": seta o cookie
            SetAuthCookie(jwt);

            return Ok(new
            {
                message = "Autenticado com sucesso.",
                token = jwt // ainda retorna se você quiser compatibilidade
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
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

            var email = request.Email?.Trim().ToLowerInvariant();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return Ok(new { message = "Se o e-mail estiver cadastrado, enviamos um link para redefinir sua senha!!" });
            }

            var resetToken = _twoFactorService.CacheResetToken(user.Id, TimeSpan.FromMinutes(10));

            var resetLink = $"https://icone.academy/reset-password?token={resetToken}";

            await _twoFactorService.SendPasswordResetEmailAsync(user.Email, resetLink);

            return Ok(new { message = "Se o e-mail estiver cadastrado, enviamos um link para redefinir sua senha." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
                return BadRequest(new { message = "Token e nova senha são obrigatórios." });

            if (!_twoFactorService.TryGetResetUserId(request.Token, out var userId))
                return BadRequest(new { message = "Token inválido ou expirado." });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return NotFound(new { message = "Usuário não encontrado." });

            user.PasswordHash = HashPassword(request.NewPassword);
            await _context.SaveChangesAsync();

            _twoFactorService.RemoveResetToken(request.Token);

            return Ok(new { message = "Senha redefinida com sucesso!" });
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
                company = user.Company == null ? null : new
                {
                    id = user.Company.Id,
                    fantasyName = user.Company.FantasyName,
                    corporateName = user.Company.CorporateName,
                    plan = user.Company.Plan,
                    isActive = user.Company.IsActive
                }
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            ClearAuthCookie();
            return NoContent();
        }
    }
}
