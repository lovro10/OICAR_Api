namespace REST_API___oicar.DTOs
{
    public class KorisnikVoznjaDTO
    {
        public int IdKorisnikVoznja { get; set; } 
    
        public int? KorisnikId { get; set; }   
        
        public int? OglasVoznjaId { get; set; } 
        
        public string? LokacijaPutnik { get; set; } 
        
        public string? LokacijaVozac { get; set; } 
    }
}
