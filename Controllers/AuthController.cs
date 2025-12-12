using icone_backend.Dtos.Auth;
using icone_backend.Dtos.Auth.Requests;
using icone_backend.Interface;
using icone_backend.Models;
using icone_backend.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace icone_backend.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthInterface _authService;

        private const string AuthCookieName = "icone_auth";

        public AuthController(IAuthInterface authService)
        {
            _authService = authService;
        }

        // --------------------------------------------------------------------
        // Helpers de Cookie
        // --------------------------------------------------------------------
        private static bool IsAcademyHost(string host)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                return false;
            }

            host = host.Trim().ToLowerInvariant();

            // cobre: icone.academy, www.icone.academy, dashboard.icone.academy, etc.
            return host == "icone.academy" || host.EndsWith(".icone.academy");
        }

        private CookieOptions BuildAuthCookieOptions(DateTimeOffset expires)
        {
            var host = HttpContext?.Request?.Host.Host ?? string.Empty;
            var isAcademy = IsAcademyHost(host);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,

                Path = "/",

                Expires = expires
            };

            if (isAcademy)
            {
                cookieOptions.Secure = true;
                cookieOptions.SameSite = SameSiteMode.None;
                cookieOptions.Domain = ".icone.academy";
            }
            else
            {
                cookieOptions.Secure = false;
                cookieOptions.SameSite = SameSiteMode.Lax;
            }

            return cookieOptions;
        }

        private void SetAuthCookie(string jwt)
        {
            
            var cookieOptions = BuildAuthCookieOptions(DateTimeOffset.UtcNow.AddDays(7));

            Response.Cookies.Append(AuthCookieName, jwt, cookieOptions);
        }

        private void ClearAuthCookie()
        {
            var deleteOptions = BuildAuthCookieOptions(DateTimeOffset.UnixEpoch);
            Response.Cookies.Delete(AuthCookieName, deleteOptions);
            Response.Cookies.Append(AuthCookieName, string.Empty, new CookieOptions
            {
                HttpOnly = deleteOptions.HttpOnly,
                Secure = deleteOptions.Secure,
                SameSite = deleteOptions.SameSite,
                Domain = deleteOptions.Domain,
                Path = deleteOptions.Path,
                Expires = DateTimeOffset.UtcNow.AddDays(-7),
                MaxAge = TimeSpan.Zero
            });
        }

        // --------------------------------------------------------------------
        // SIGNUP
        // --------------------------------------------------------------------
        [HttpPost("signup")]
        public async Task<IActionResult> Register([FromBody] RegisterAccountRequest request)
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

            var result = await _authService.RegisterAsync(request);

            if (!result.Success)
            {
                var err = result.Error ?? new Error
                {
                    Code = "UNKNOWN_ERROR",
                    Message = "Erro ao criar usuário.",
                    TraceId = HttpContext.TraceIdentifier
                };

                return err.Code switch
                {
                    "EMAIL_NOT_VERIFIED" => BadRequest(err),
                    "DUPLICATE_EMAIL" => BadRequest(err),
                    "VALIDATION_ERROR" => BadRequest(err),
                    _ => StatusCode(500, err)
                };
            }

            if (!string.IsNullOrEmpty(result.Token))
            {
                SetAuthCookie(result.Token);
            }

            return Ok(new
            {
                message = "Usuário criado. Prossiga para próxima etapa.",
                token = result.Token
            });
        }

        // --------------------------------------------------------------------
        // SIGNUP - EMAIL CODE
        // --------------------------------------------------------------------
        [HttpPost("signup/email-code")]
        public async Task<IActionResult> SendSignupEmailCode(
            [FromServices] TwoFactorService twoFactorService,
            [FromBody] SignupEmailCodeRequest request)
        {
            var email = request.Email?.Trim().ToLowerInvariant();

            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new { message = "E-mail é obrigatório." });
            }

            await twoFactorService.SendSignupEmailCodeAsync(email);

            return Ok(new { message = "Enviamos um código de verificação para o seu e-mail." });
        }

        [HttpPost("signup/email-code/verify")]
        public async Task<IActionResult> VerifySignupEmailCode(
            [FromServices] TwoFactorService twoFactorService,
            [FromBody] VerifySignupEmailCodeRequest request)
        {
            var email = request.Email?.Trim().ToLowerInvariant();
            var code = request.Code?.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(code))
            {
                return BadRequest(new { message = "E-mail e código são obrigatórios.", valid = false });
            }

            var valid = await twoFactorService.VerifySignupEmailCodeAsync(email, code);

            if (!valid)
            {
                return Ok(new { valid = false });
            }

            return Ok(new { valid = true });
        }

        // --------------------------------------------------------------------
        // LOGIN (1ª etapa: senha -> manda código 2FA)
        // --------------------------------------------------------------------
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);

            if (!result.Success)
            {
                var err = result.Error ?? new Error
                {
                    Code = "INVALID_CREDENTIALS",
                    Message = "E-mail ou senha inválidos.",
                    TraceId = HttpContext.TraceIdentifier
                };

                return Unauthorized(err);
            }

            return Ok(new
            {
                message = "Código de verificação enviado para seu e-mail.",
                requires2FA = result.Requires2FA,
                twoFactorToken = result.TwoFactorToken
            });
        }

        // --------------------------------------------------------------------
        // VERIFY 2FA (2ª etapa: código -> gera JWT final)
        // --------------------------------------------------------------------
        [HttpPost("verify-2fa")]
        public async Task<IActionResult> VerifyTwoFactor([FromBody] VerifyTwoFactorRequest request)
        {
            var result = await _authService.VerifyTwoFactorAsync(request);

            if (!result.Success || string.IsNullOrEmpty(result.Jwt))
            {
                var err = result.Error ?? new Error
                {
                    Code = "INVALID_2FA_CODE",
                    Message = "Código de verificação inválido.",
                    TraceId = HttpContext.TraceIdentifier
                };

                return BadRequest(err);
            }

            SetAuthCookie(result.Jwt);

            return Ok(new
            {
                message = "Autenticado com sucesso.",
                token = result.Jwt
            });
        }

        // --------------------------------------------------------------------
        // FORGOT PASSWORD
        // --------------------------------------------------------------------
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
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

            var result = await _authService.ForgotPasswordAsync(request);

            if (!result.Success)
            {
                return StatusCode(500, new Error
                {
                    Code = "INTERNAL_ERROR",
                    Message = "Erro ao processar solicitação de redefinição de senha.",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            return Ok(new
            {
                message = "Se o e-mail estiver cadastrado, enviamos um link para redefinir sua senha."
            });
        }

        // --------------------------------------------------------------------
        // RESET PASSWORD
        // --------------------------------------------------------------------
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var result = await _authService.ResetPasswordAsync(request);

            if (!result.Success)
            {
                var err = result.Error ?? new Error
                {
                    Code = "INVALID_RESET_TOKEN",
                    Message = "Token inválido ou expirado.",
                    TraceId = HttpContext.TraceIdentifier
                };

                return err.Code switch
                {
                    "USER_NOT_FOUND" => NotFound(err),
                    "INVALID_RESET_TOKEN" => BadRequest(err),
                    "VALIDATION_ERROR" => BadRequest(err),
                    _ => StatusCode(500, err)
                };
            }

            return Ok(new { message = "Senha redefinida com sucesso!" });
        }

        // --------------------------------------------------------------------
        // ME
        // --------------------------------------------------------------------
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

            var me = await _authService.GetMeAsync(userId);

            if (me == null)
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
                id = me.Id,
                firstName = me.FirstName,
                lastName = me.LastName,
                email = me.Email,
                role = me.Role,
                companyId = me.CompanyId,
                isActive = me.IsActive,
                company = me.Company == null ? null : new
                {
                    id = me.Company.Id,
                    fantasyName = me.Company.FantasyName,
                    corporateName = me.Company.CorporateName,
                    plan = me.Company.Plan,
                    isActive = me.Company.IsActive
                }
            });
        }

        // --------------------------------------------------------------------
        // LOGOUT
        // --------------------------------------------------------------------
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            ClearAuthCookie();
            return NoContent();
        }
    }
}