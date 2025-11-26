using System.ComponentModel.DataAnnotations;

namespace icone_backend.Dtos.Address
{
    public class AddressDto
    {
        [Required(ErrorMessage = "O país é obrigatório.")]
        public string CountryCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "O CEP é obrigatório.")]
        public string PostalCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "O estado é obrigatório.")]
        public string StateRegion { get; set; } = string.Empty;

        [Required(ErrorMessage = "A cidade é obrigatória.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "O endereço linha 1 é obrigatório.")]
        public string Line1 { get; set; } = string.Empty;
        public string? Line2 { get; set; }
    }
}
