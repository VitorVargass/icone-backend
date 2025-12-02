using icone_backend.Attributes;
using System.ComponentModel.DataAnnotations;

namespace icone_backend.Dtos.Auth
{
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "The token is required.")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "The new password is required.")]
        [Password(MinLength = 8)]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirming your password is required.")]
        [Compare("NewPassword", ErrorMessage = "The passwords don't match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
