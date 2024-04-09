using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kheti.Models
{
    public class Payment
    {
        [Key]
        public Guid Id { get; set; }
        [ForeignKey("UserId")]
        public string UserId { get; set; }
        public KhetiApplicationUser User { get; set; }
        public string TransactionId { get; set; }
        public string PaymentMethod {  get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        [ForeignKey("OrderId")]
        public int? OrderId { get; set; }
        public Order Order { get; set; }
        [ForeignKey("BookingId")]
        public Guid? BookingId { get; set; }
        public Booking? Booking { get; set; }

    }
}
