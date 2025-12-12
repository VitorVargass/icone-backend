using BCrypt.Net;
using icone_backend.Data;
using icone_backend.Dtos.Auth;
using icone_backend.Dtos.Auth.Requests;
using icone_backend.Dtos.Auth.Responses;
using icone_backend.Interface;
using icone_backend.Models;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace icone_backend.Services.Auth
{
    public class AuthService : IAuthInterface
    {
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;
        private readonly TwoFactorService _twoFactorService;

        public AuthService(
            AppDbContext context,
            TokenService tokenService,
            TwoFactorService twoFactorService)
        {
            _context = context;
            _tokenService = tokenService;
            _twoFactorService = twoFactorService;
        }

        // --------------------------------------------------------------------
        // Helpers internos
        // --------------------------------------------------------------------
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 10);
        }

        private bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        // --------------------------------------------------------------------
        // REGISTER
        // --------------------------------------------------------------------
        public async Task<RegisterResult> RegisterAsync(RegisterAccountRequest request)
        {
            try
            {
                var email = (request.Email ?? string.Empty).Trim().ToLowerInvariant();

                if (string.IsNullOrEmpty(email) || !_twoFactorService.IsSignupEmailVerified(email))
                {
                    return new RegisterResult
                    {
                        Success = false,
                        Error = new Error
                        {
                            Code = "EMAIL_NOT_VERIFIED",
                            Message = "Você precisa verificar o e-mail antes de continuar.",
                            Field = "email"
                        }
                    };
                }

                if (!string.IsNullOrEmpty(email) &&
                    await _context.Users.AnyAsync(u => u.Email == email))
                {
                    return new RegisterResult
                    {
                        Success = false,
                        Error = new Error
                        {
                            Code = "DUPLICATE_EMAIL",
                            Message = "E-mail já cadastrado.",
                            Field = "email"
                        }
                    };
                }

                var user = new UserModel
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = email,
                    PasswordHash = HashPassword(request.Password),
                    Role = "admin",
                    Plan = request.Plan,
                    IsActive = true,
                    CompanyId = null,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var token = _tokenService.GenerateToken(user);

                return new RegisterResult
                {
                    Success = true,
                    Token = token
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERRO EM AuthService.RegisterAsync:");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                return new RegisterResult
                {
                    Success = false,
                    Error = new Error
                    {
                        Code = "INTERNAL_ERROR",
                        Message = "Erro interno ao criar usuário."
                    }
                };
            }
        }

        // --------------------------------------------------------------------
        // LOGIN (1ª etapa)
        // --------------------------------------------------------------------
        public async Task<LoginResult> LoginAsync(LoginRequest request)
        {
            var email = request.Email?.Trim().ToLowerInvariant();

            var user = await _context.Set<UserModel>()
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            {
                return new LoginResult
                {
                    Success = false,
                    Error = new Error
                    {
                        Code = "INVALID_CREDENTIALS",
                        Message = "E-mail ou senha inválidos."
                    }
                };
            }

            var now = DateTimeOffset.UtcNow;
            user.LastLoginAt = now;
            await _context.SaveChangesAsync();

            var twoFactorToken = await _twoFactorService.GenerateAndSendCodeAsync(user.Id, user.Email);

            return new LoginResult
            {
                Success = true,
                Requires2FA = true,
                TwoFactorToken = twoFactorToken
            };
        }

        // --------------------------------------------------------------------
        // VERIFY 2FA (2ª etapa)
        // --------------------------------------------------------------------
        public async Task<VerifyTwoFactorResult> VerifyTwoFactorAsync(VerifyTwoFactorRequest request)
        {
            var result = await _twoFactorService.ValidateCodeAsync(request.TwoFactorToken, request.Code);
            if (!result.Success)
            {
                return new VerifyTwoFactorResult
                {
                    Success = false,
                    Error = new Error
                    {
                        Code = "INVALID_2FA_CODE",
                        Message = "Código de verificação inválido."
                    }
                };
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == result.UserId);
            if (user == null)
            {
                return new VerifyTwoFactorResult
                {
                    Success = false,
                    Error = new Error
                    {
                        Code = "USER_NOT_FOUND",
                        Message = "Usuário não encontrado."
                    }
                };
            }

            var now = DateTimeOffset.UtcNow;
            user.LastTwoFactorVerifiedAt = now;
            user.LastLoginAt = now;
            await _context.SaveChangesAsync();

            var jwt = _tokenService.GenerateToken(user);

            return new VerifyTwoFactorResult
            {
                Success = true,
                Jwt = jwt
            };
        }

        // --------------------------------------------------------------------
        // FORGOT PASSWORD
        // --------------------------------------------------------------------
        public async Task<ForgotPasswordResult> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var email = request.Email?.Trim().ToLowerInvariant();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                // mesmo comportamento: não informa se o email existe
                return new ForgotPasswordResult { Success = true };
            }

            var resetToken = _twoFactorService.CacheResetToken(user.Id, TimeSpan.FromMinutes(10));
            var resetLink = $"https://icone.academy/reset-password?token={resetToken}";

            await _twoFactorService.SendPasswordResetEmailAsync(user.Email, resetLink);

            return new ForgotPasswordResult { Success = true };
        }

        // --------------------------------------------------------------------
        // RESET PASSWORD
        // --------------------------------------------------------------------
        public async Task<ResetPasswordResult> ResetPasswordAsync(ResetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Token) ||
                string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return new ResetPasswordResult
                {
                    Success = false,
                    Error = new Error
                    {
                        Code = "VALIDATION_ERROR",
                        Message = "Token e nova senha são obrigatórios."
                    }
                };
            }

            if (!_twoFactorService.TryGetResetUserId(request.Token, out var userId))
            {
                return new ResetPasswordResult
                {
                    Success = false,
                    Error = new Error
                    {
                        Code = "INVALID_RESET_TOKEN",
                        Message = "Token inválido ou expirado."
                    }
                };
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return new ResetPasswordResult
                {
                    Success = false,
                    Error = new Error
                    {
                        Code = "USER_NOT_FOUND",
                        Message = "Usuário não encontrado."
                    }
                };
            }

            user.PasswordHash = HashPassword(request.NewPassword);
            await _context.SaveChangesAsync();

            _twoFactorService.RemoveResetToken(request.Token);

            return new ResetPasswordResult { Success = true };
        }

        // --------------------------------------------------------------------
        // ME
        // --------------------------------------------------------------------
        public async Task<MeResult?> GetMeAsync(long userId)
        {
            var user = await _context.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return null;

            return new MeResult
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                CompanyId = user.CompanyId,
                Company = user.Company == null
                    ? null
                    : new CompanySummary
                    {
                        Id = user.Company.Id,
                        FantasyName = user.Company.FantasyName,
                        CorporateName = user.Company.CorporateName,
                        Plan = user.Company.Plan,
                        IsActive = user.Company.IsActive
                    }
            };
        }
    }
}
