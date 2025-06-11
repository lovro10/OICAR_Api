using REST_API___oicar.DTOs;

namespace REST_API___oicar.DTOs
{
    public class VoziloDetaljiDTO
    {
        public VoziloDTO Vozilo { get; set; }
        public List<ImageDTO> IdentificationImages { get; set; }
    }
}
