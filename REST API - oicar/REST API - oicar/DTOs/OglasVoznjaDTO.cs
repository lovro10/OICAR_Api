using System;
using System.ComponentModel.DataAnnotations;

namespace REST_API___oicar.DTOs
{
    public class OglasVoznjaDTO
    {
        public int IdOglasVoznja { get; set; }

        public int? VoziloId { get; set; }
        public string? Marka { get; set; }
        public string? Model { get; set; }
        public string? Registracija { get; set; }

        public DateTime DatumIVrijemePolaska { get; set; }
        public DateTime DatumIVrijemeDolaska { get; set; }

        public int? TroskoviId { get; set; }
        public decimal? Cestarina { get; set; }
        public decimal? Gorivo { get; set; }

        public decimal? CestarinaPoPutniku { get; set; }
        public decimal? GorivoPoPutniku { get; set; }

        public int BrojPutnika { get; set; }

        public int? StatusVoznjeId { get; set; }
        public string? StatusVoznjeNaziv { get; set; }

        public int? LokacijaId { get; set; }
        public string? Polaziste { get; set; }
        public string? Odrediste { get; set; }
    }
}
