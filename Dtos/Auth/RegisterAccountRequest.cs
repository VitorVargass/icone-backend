using icone_backend.Attributes;
using icone_backend.Dtos.Address;
using System.ComponentModel.DataAnnotations;

namespace icone_backend.Dtos.Auth
{
    public class RegisterAccount
    {
     // ------------------------- USER INFO -------------------------

        [Required(ErrorMessage = "The name is required.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "The last name is required.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "The plan is required.")]
        public string Plan { get; set; } = "starter";

        [Required(ErrorMessage = "The password is required")]
        [Password(MinLength = 8)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirming your password is required.")]
        [Compare("Password", ErrorMessage = "The passwords don't match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

     /* ------------------------- COMPANY INFO -------------------------

        [Required(ErrorMessage = "The company document is required")]
        [Document(AllowDotsAndDashes = false)]
        public string DocumentCompany { get; set; } = string.Empty;

        [Required(ErrorMessage = "The fantasy name is required")]
        public string FantasyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "The corporate name is required.")]
        public string CorporateName { get; set; } = string.Empty;

        [Required(ErrorMessage = "The phone is required")]
        [Phone(ErrorMessage = "Invalid phone")]
        public string Phone { get; set; } = string.Empty;

        public string? Website { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public AddressDto Address { get; set; } = null!;
        */
    }
}
