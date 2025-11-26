using icone_backend.Attributes;
using System.ComponentModel.DataAnnotations;

namespace icone_backend.Dtos.Address
{
    public class AddressDto
    {
        [Required(ErrorMessage = "O país é obrigatório.")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "O código do país deve ter exatamente 2 letras.")]
        [RegularExpression("^[A-Za-z]{2}$", ErrorMessage = "O código do país deve conter apenas letras.")]
        public string CountryCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "O CEP é obrigatório.")]
        [Document(AllowDotsAndDashes = false)]
        public string PostalCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "O estado é obrigatório.")]
        [RegionByCountryAttribute]
        public string StateRegion { get; set; } = string.Empty;

        
        [Required(ErrorMessage = "A cidade é obrigatória.")]
        [RegularExpression(@"^[\p{L}\p{M}\s\-']+$", ErrorMessage = "O nome da cidade contém caracteres inválidos.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "O endereço linha 1 é obrigatório.")]
        public string Line1 { get; set; } = string.Empty;
        public string? Line2 { get; set; }
    }
}
