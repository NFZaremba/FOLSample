using System.ComponentModel.DataAnnotations;

namespace FutureOfLatinos.Models.Requests
{
    public class LinkedInAddRequest: RegistrationAddRequest
    {
        [Required, MaxLength(255)]
        public string FirstName { get; set; }
        [Required, MaxLength(255)]
        public string LastName { get; set; }
        [Required]
        public string ProfileUrl { get; set; }
    }
}
