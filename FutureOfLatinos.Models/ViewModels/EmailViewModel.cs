using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FutureOfLatinos.Models.ViewModels
{
    public class EmailViewModel
    {
        public int Id { get; set; }

        [Required, MaxLength(255), MinLength(3)]
        public string Email { get; set; }
        public bool isConfirmed { get; set; }
    }
}
