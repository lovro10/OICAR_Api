namespace REST_API___oicar.DTOs
{
    public class KorisnikUpdateDTO
    {
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Email { get; set; }
        public string Telefon { get; set; }
        public DateOnly DatumRodjenja { get; set; }
        public string Username { get;  set; }
    }
}
