namespace REST_API___oicar.DTOs
{
    public class RegistracijaPutnikDTO
    {
        public int Id { get; set; }

        public string? Username { get; set; }

        public string? Password { get; set; }

        public string? Ime { get; set; }

        public string? Prezime { get; set; }

        public string? Email { get; set; }

        public string? Telefon { get; set; }

        public DateOnly Datumrodjenja { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? Osobna { get; set; }

        public string? Selfie { get; set; }
    }
}
