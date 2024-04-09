using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kheti.Models
{
    public class Invoice
    {
        [Key]
        public Guid InvoiceId { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        public string PaymentStatus { get; set; }
        [ForeignKey("OrderId")]
        public int OrderId { get; set; }
        public Order Order { get; set; }

       
    }
}
