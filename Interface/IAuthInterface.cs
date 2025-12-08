using icone_backend.Dtos.Auth.Requests;
using icone_backend.Dtos.Auth.Responses;    

namespace icone_backend.Interface
{
    public interface IAuthInterface
    {
        Task<RegisterResult> RegisterAsync(RegisterAccountRequest request);
        Task<LoginResult> LoginAsync(LoginRequest request);
        Task<VerifyTwoFactorResult> VerifyTwoFactorAsync(VerifyTwoFactorRequest request);
        Task<ForgotPasswordResult> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<ResetPasswordResult> ResetPasswordAsync(ResetPasswordRequest request);
        Task<MeResult?> GetMeAsync(long userId);
    }
}
