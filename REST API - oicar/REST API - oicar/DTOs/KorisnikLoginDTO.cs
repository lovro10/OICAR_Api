using System.ComponentModel.DataAnnotations;

namespace REST_API___oicar.DTOs
{
    public class KorisnikLoginDTO
    {
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;
    }
}
