using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FutureOfLatinos.Models.ViewModels
{
    public class AuthTokenViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        [Required, MaxLength(255), MinLength(3)]
        public string Email { get; set; }
        public string ConfirmationAuthToken { get; set; }
        public string Salt { get; set; }
        public string Pass { get; set; }
        public bool isConfirmed { get; set; }
        public bool isActive { get; set; }
        public string Password { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
