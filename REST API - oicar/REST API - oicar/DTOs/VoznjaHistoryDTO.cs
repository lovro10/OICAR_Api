namespace REST_API___oicar.DTOs
{
    public class VoznjaHistoryDTO
    {
        public int Korisnikvoznjaid { get; set; }

        public int? Oglasvoznjaid { get; set; }

        public DateTime DatumVoznje { get; set; }

        public string? Polaziste { get; set; }

        public string? Odrediste { get; set; }

        public string? Lokacijavozac { get; set; }

        public string? Lokacijaputnik { get; set; }
    }
}
