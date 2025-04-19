namespace REST_API___oicar.DTOs
{
    public class PorukaDTO
    {
        public int PorukaId  { get; set; } 
        
        public int KorisnikVoznjaId { get; set; }   
        
        public int? PutnikId { get; set; }          
        
        public int? VozacId { get; set; }

        public string Content { get; set; } = null!; 
    }
}
