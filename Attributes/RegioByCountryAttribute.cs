using System.ComponentModel.DataAnnotations;

namespace icone_backend.Attributes
{
    public class RegionByCountryAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var state = value as string ?? string.Empty;

            var countryCodeProp = validationContext.ObjectType.GetProperty("CountryCode");
            var countryCode = countryCodeProp?.GetValue(validationContext.ObjectInstance) as string ?? string.Empty;
            countryCode = countryCode.ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(state))
                return new ValidationResult("O estado/região é obrigatório.");

            // Países que usam código de 2 letras
            var twoLetterCountries = new[] { "BR", "US", "CA", "AU" };

            if (twoLetterCountries.Contains(countryCode))
            {
                if (state.Length != 2)
                    return new ValidationResult($"O estado deve ter exatamente 2 letras para o país {countryCode}.");
                if (!System.Text.RegularExpressions.Regex.IsMatch(state, @"^[A-Za-z]{2}$"))
                    return new ValidationResult("O estado deve conter apenas letras.");
            }
            else
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(state, @"^[\p{L}\p{M}\s\-']+$"))
                    return new ValidationResult("O estado/região contém caracteres inválidos.");
                if (state.Length < 2)
                    return new ValidationResult("O estado/região deve ter pelo menos 2 caracteres.");
            }

            return ValidationResult.Success;
        }
    }
}
