using System;
using System.ComponentModel.DataAnnotations;

namespace REST_API___oicar.DTOs
{
    public class OglasVoziloDTO 
    { 
        public int IdOglasVozilo { get; set; }

        public int? VoziloId { get; set; }
        public string? Marka { get; set; }
        public string? Model { get; set; }
        public string? Registracija { get; set; }

        public DateTime DatumPocetkaRezervacije { get; set; }
        public DateTime DatumZavrsetkaRezervacije { get; set; }

        public int? KorisnikId { get; set; }
        public string? Username { get; set; }
        public string? Ime { get; set; }
        public string? Prezime { get; set; }
        public string? Email { get; set; }

    }
}
