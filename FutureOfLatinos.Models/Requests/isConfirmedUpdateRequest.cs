using System.ComponentModel.DataAnnotations;

namespace FutureOfLatinos.Models.Requests
{
    public class isConfirmedUpdateRequest
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public bool isConfirmed { get; set; }
    }
}
