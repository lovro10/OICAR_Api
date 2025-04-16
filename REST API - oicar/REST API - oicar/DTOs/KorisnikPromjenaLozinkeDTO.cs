using System.ComponentModel.DataAnnotations;

namespace REST_API___oicar.DTOs
{
    public class KorisnikPromjenaLozinkeDTO
    {
        [Required(ErrorMessage = "Username je obavezan.")]
        [EmailAddress(ErrorMessage = "Neispravan email format.")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Stara lozinka je obavezna.")]
        public string? OldPassword { get; set; }

        [Required(ErrorMessage = "Nova lozinka je obavezna.")]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [Required(ErrorMessage = "Potvrda nove lozinke je obavezna.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Lozinke se ne podudaraju.")]
        public string? ConfirmNewPassword { get; set; }
    }
}
