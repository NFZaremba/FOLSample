using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FutureOfLatinos.Models.Domain
{
    public class AuthToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ConfirmationAuthToken { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
