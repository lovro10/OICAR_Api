namespace REST_API___oicar.DTOs
{
    public class PorukaGetDTO
    {
        public int Idporuka { get; set; }

        public int? PutnikId { get; set; }

        public int? VozacId { get; set; }

        public int KorisnikVoznjaId { get; set; }

        public string? SenderName { get; set; } 

        public string? Content { get; set; } 
    }
}
