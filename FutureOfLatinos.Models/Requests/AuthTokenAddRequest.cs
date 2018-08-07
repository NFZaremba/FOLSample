using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FutureOfLatinos.Models.Requests
{
    public class AuthTokenAddRequest
    {
        public int UserId { get; set; }
        [Required, MaxLength(255), MinLength(3)]
        public string Email { get; set; }
        public Guid ConfirmationToken { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
