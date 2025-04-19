using System.ComponentModel.DataAnnotations;

namespace REST_API___oicar.DTOs
{
    public class VoziloDTO
    {
        public int Id { get; set; }

        public string? Marka { get; set; }

        public string? Model { get; set; }

        public string? Registracija { get; set; }

        public string? Prometna { get; set; }
    }
}
