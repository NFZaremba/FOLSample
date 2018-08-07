using System.ComponentModel.DataAnnotations;

namespace FutureOfLatinos.Models.Requests
{
    public class LoginRequest: ReCaptchaAddRequest
    {
        [Required, MaxLength(255), MinLength(3)]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
