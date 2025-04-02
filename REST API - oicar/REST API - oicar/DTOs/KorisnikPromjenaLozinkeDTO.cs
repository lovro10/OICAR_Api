using System.ComponentModel.DataAnnotations;

namespace REST_API___oicar.DTOs
{
    public class KorisnikPromjenaLozinkeDTO
    {
        [Required(ErrorMessage = "Email je obavezan.")]
        [EmailAddress(ErrorMessage = "Neispravan email format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Stara lozinka je obavezna.")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nova lozinka je obavezna.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Potvrda nove lozinke je obavezna.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Lozinke se ne podudaraju.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
