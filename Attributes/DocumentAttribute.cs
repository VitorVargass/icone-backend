using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace icone_backend.Attributes
{
    public class DocumentAttribute : ValidationAttribute
    {
        public bool AllowDotsAndDashes { get; set; } = false;

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var document = value as string ?? string.Empty;

            if (string.IsNullOrWhiteSpace(document))
                return new ValidationResult("O documento é obrigatório.");

            
            var pattern = AllowDotsAndDashes ? @"^[0-9.\-]+$" : @"^[0-9]+$";

            if (!Regex.IsMatch(document, pattern))
                return new ValidationResult("O documento deve conter apenas números.");

            return ValidationResult.Success;
        }
    }
}
