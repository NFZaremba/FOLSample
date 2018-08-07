using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FutureOfLatinos.Models.Requests
{
    public class EmailConfirmationUpdateRequest
    {
        public int Id { get; set; }
        public bool isConfirmed { get; set; }
    }
}
