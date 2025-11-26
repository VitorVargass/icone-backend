using icone_backend.Attributes;
using System.ComponentModel.DataAnnotations;

namespace icone_backend.Dtos.Auth
{
    public class RegisterUserStepRequest
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [RegularExpression(@"^[\p{L}\p{M}\s\-']+$", ErrorMessage = "O nome contém caracteres inválidos.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "O sobrenome é obrigatório.")]
        [RegularExpression(@"^[\p{L}\p{M}\s\-']+$", ErrorMessage = "O sobrenome contém caracteres inválidos.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "O documento é obrigatório.")]
        [Document(AllowDotsAndDashes = false)]
        public string Document { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [Password(MinLength = 8)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "A confirmação de senha é obrigatória.")]
        [Compare("Password", ErrorMessage = "As senhas não coincidem.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
