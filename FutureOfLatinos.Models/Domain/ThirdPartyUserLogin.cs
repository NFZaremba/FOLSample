using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FutureOfLatinos.Models.Domain
{
    public class ThirdPartyUserLogin
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; }
        public bool isConfirmed { get; set; }
        public bool isActive { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string MiddleInitial { get; set; }
        public string LastName { get; set; }
        public string Location { get; set; }
        public string CreatedBy { get; set; }
        public int ThirdPartyTypeId { get; set; }
        public string AccountId { get; set; }
        public int PersonId { get; set; }
        public int FileStorageId { get; set; }

    }
}
