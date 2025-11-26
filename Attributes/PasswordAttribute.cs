using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace icone_backend.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class PasswordAttribute : ValidationAttribute
    {
        public int MinLength { get; set; } = 8;

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var password = value as string;

            if (string.IsNullOrWhiteSpace(password))
            {
                return new ValidationResult("A senha é obrigatória.");
            }

            if (password.Length < MinLength)
                return new ValidationResult($"A senha deve ter pelo menos {MinLength} caracteres.");

            if (!Regex.IsMatch(password, "[a-z]"))
                return new ValidationResult("A senha deve conter pelo menos uma letra minúscula.");

            if (!Regex.IsMatch(password, "[A-Z]"))
                return new ValidationResult("A senha deve conter pelo menos uma letra maiúscula.");

            if (!Regex.IsMatch(password, "[0-9]"))
                return new ValidationResult("A senha deve conter pelo menos um número.");

            if (!Regex.IsMatch(password, "[^a-zA-Z0-9]"))
                return new ValidationResult("A senha deve conter pelo menos um caractere especial.");

            return ValidationResult.Success;
        }
    }
}
