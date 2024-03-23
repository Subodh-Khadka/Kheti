using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kheti.Models
{
    public class Booking
    {
        public int BookingId { get; set; }
        [ForeignKey ("ProductId")]
        public int ProductId { get; set; }
        public Product Product { get; set; }
            
        [ForeignKey ("UserId")]
        public string UserId { get; set; }
        public KhetiApplicationUser User {  get; set; }
        [Required]
        public DateTime RequestStartDate { get; set; }
        [Required]
        public DateTime RequestEndDate { get; set;}
        public DateTime? ActualRequestStartDate { get; set; }
        public DateTime? ActualRequestEndDate { get; set; }
        public string bookingStatus { get; set; }
        public string PaymentStatus { get; set; }


    }
}
