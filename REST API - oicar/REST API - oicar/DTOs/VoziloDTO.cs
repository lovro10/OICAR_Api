namespace REST_API___oicar.DTOs
{
    public class VoziloDTO
    {
        public int Idvozilo { get; set; }

        public string? Naziv { get; set; }

        public string? Marka { get; set; } 

        public string? Model { get; set; } 

        public string? Registracija { get; set; } 

        public int? VozacId { get; set; }
    
        public bool? Isconfirmed { get; set; } 

        public string? FrontImageBase64 { get; set; } 
        public string? BackImageBase64 { get; set; } 

        public string? FrontImageName { get; set; } 
        public string? BackImageName { get; set; }

        public VozacDTO? Vozac { get; set; }
    }
}
