using REST_API___oicar.Models;

namespace REST_API___oicar.DTOs
{
    public class KorisnikDTO
    {
        public int IDKorisnik { get; set; }

        public string? Ime { get; set; }

        public string? Prezime { get; set; }

        public string? Email { get; set; }

        public string? Username { get; set; }

        public string? Telefon { get; set; }

        public DateOnly? DatumRodjenja { get; set; }

        public virtual Uloga? Uloga { get; set; }

        public string Pwdhash { get; internal set; }

        public string Pwdsalt { get; internal set; }

        public int? UlogaId { get; internal set; }

        public bool? Isconfirmed { get; set; }

        public List<ImageDTO> ImagesType1 { get; set; } = new List<ImageDTO>();
        public List<ImageDTO> ImagesType2 { get; set; } = new List<ImageDTO>();
        public List<ImageDTO> ImagesType3 { get; set; } = new List<ImageDTO>();
    }
}
