using System;
using System.ComponentModel.DataAnnotations;

namespace REST_API___oicar.DTOs
{
    public class OglasVoznjaDTO
    {
        [Required(ErrorMessage = "VoziloId je obavezan.")]
        public int VoziloId { get; set; }

        [Required(ErrorMessage = "Datum i vrijeme polaska je obavezan.")]
        public DateTime DatumIVrijemePolaska { get; set; }

        [Required(ErrorMessage = "Datum i vrijeme dolaska je obavezan.")]
        public DateTime DatumIVrijemeDolaska { get; set; }

        [Required(ErrorMessage = "Broj putnika je obavezan.")]
        public int BrojPutnika { get; set; }

        [Required(ErrorMessage = "Polazište je obavezno.")]
        public string LokacijaPolaziste { get; set; } = string.Empty;

        [Required(ErrorMessage = "Odredište je obavezno.")]
        public string LokacijaOdrediste { get; set; } = string.Empty;
    }
}
