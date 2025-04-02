using System.ComponentModel.DataAnnotations;

namespace REST_API___oicar.DTOs
{
    public class VoziloDTO
    {
        [Required(ErrorMessage = "Marka je obavezna.")]
        public string Marka { get; set; } = string.Empty;

        [Required(ErrorMessage = "Model je obavezan.")]
        public string Model { get; set; } = string.Empty;

        [Required(ErrorMessage = "Registracija je obavezna.")]
        public string Registracija { get; set; } = string.Empty;

        public int Imageprometnaid { get; set; } = 0;
    }
}
