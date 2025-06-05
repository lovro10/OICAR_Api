using System;
using System.ComponentModel.DataAnnotations;

namespace REST_API___oicar.DTOs
{
    public class KorisnikRegistracijaDTO
    {
        public int Id { get; set; } 
    
        public string? Ime { get; set; } 

        public string? Prezime { get; set; }

        public string? Username { get; set; }

        public string? Password { get; set; }

        public string? Email { get; set; }

        public string? Telefon { get; set; }
        
        public DateOnly Datumrodjenja { get; set; }

        public int UlogaId { get; set; } 
    }
}
