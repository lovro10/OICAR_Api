namespace REST_API___oicar.DTOs
{
    public class VehicleReservationDTO
    {
        public int KorisnikId { get; set; }
        public int OglasVoziloId { get; set; }

        public DateTime DatumPocetkaRezervacije { get; set; }
        public DateTime DatumZavrsetkaRezervacije { get; set; }

        public DateTime DozvoljeniPocetak { get; set; }
        public DateTime DozvoljeniKraj { get; set; }
        public List<DateTime> ReservedDates { get; set; } = new();
    }
}
