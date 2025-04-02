using System;
using System.ComponentModel.DataAnnotations;

namespace REST_API___oicar.DTOs
{
    public class KorisnikRegistracijaDTO
    {
        [Required(ErrorMessage = "Ime je obavezno.")]
        public string Ime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Prezime je obavezno.")]
        public string Prezime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Datum rođenja je obavezan.")]
        public DateOnly Datumrodjenja { get; set; }

        [Required(ErrorMessage = "Email je obavezan.")]
        [EmailAddress(ErrorMessage = "Neispravna email adresa.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username je obavezan.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lozinka je obavezna.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Potvrda lozinke je obavezna.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Lozinke se ne podudaraju.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? Telefon { get; set; }

        /// Uloga: default 1 = regular user, 2 = admin
        public int Uloga { get; set; } = 1;
    }
}
