using FutureOfLatinos.Models.Requests;
using System;

namespace FutureOfLatinos.Models.Domain
{
    public class Registration : RegistrationAddRequest
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
