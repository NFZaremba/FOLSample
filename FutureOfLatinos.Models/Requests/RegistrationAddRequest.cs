using System.ComponentModel.DataAnnotations;

namespace FutureOfLatinos.Models.Requests
{
    public class RegistrationAddRequest: ReCaptchaAddRequest
    {
        [Required, MaxLength(128), MinLength(3)]
        public string FirstName { get; set; }
        [Required, MaxLength(128), MinLength(3)]
        public string LastName { get; set; }
        [Required, MaxLength(255), MinLength(3)]
        public string Email { get; set; }
        [Required, MaxLength(20), MinLength(8)]
        public string Password { get; set; }
        public string Salt { get; set; }
        public bool isConfirmed { get; set; }
        public bool isActive { get; set; }
    }
}
