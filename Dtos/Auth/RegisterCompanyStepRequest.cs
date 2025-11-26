using icone_backend.Dtos.Address;
using System.ComponentModel.DataAnnotations;

namespace icone_backend.Dtos.Auth
{
    public class RegisterCompanyStepRequest
    {
        [Required(ErrorMessage = "O ID do usuário é obrigatório.")]
        public long UserId { get; set; }

        [Required(ErrorMessage = "O documento da empresa é obrigatório.")]
        public string Document { get; set; } = string.Empty;

        [Required(ErrorMessage = "O nome fantasia é obrigatório.")]
        public string FantasyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "A razão social é obrigatória.")]
        public string CorporateName { get; set; } = string.Empty;

        [Required(ErrorMessage = "O telefone é obrigatório.")]
        [Phone(ErrorMessage = "O número de telefone é inválido.")]
        public string Phone { get; set; } = string.Empty;

        [Url(ErrorMessage = "O site deve ser uma URL válida.")]
        public string? Website { get; set; }

        [Required(ErrorMessage = "O endereço é obrigatório.")]
        public AddressDto Address { get; set; } = null!;

    }
}
