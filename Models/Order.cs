using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kheti.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        [ValidateNever]
        public KhetiApplicationUser User { get; set; }
        [Required]
        public DateTime OrderCreatedDate { get; set; }
        public DateTime ShippedDate { get; set; }
        [Required]
        public decimal OrderTotal {  get; set; }    
        public string? OrderStatus { get; set; }
        public string? PaymentStatus { get; set; }

        public DateTime PaymentDate { get; set; }

        public string? OrderPaymentId{ get; set; }

        [Required]
        public string CustomerName { get; set; }
        [Required]
        public string Address { get; set; }
        public string? LocalAddress { get; set; }
        [Required]
        public string phoneNumber { get; set; }

    }
}
