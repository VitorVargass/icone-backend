using icone_backend.Models;
using System.ComponentModel.DataAnnotations;

namespace icone_backend.Dtos.Auth
{
    public class SignupEmailCodeRequest
    {
        [Required(ErrorMessage = "Email required.")]
        public string Email { get; set; } = string.Empty;
    }
}
