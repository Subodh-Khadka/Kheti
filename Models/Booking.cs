using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kheti.Models
{
    public class Booking
    {
        public Guid BookingId { get; set; }
        [ForeignKey("ProductId")]
        public Guid ProductId { get; set; }
        public Product Product { get; set; }

        [ForeignKey("UserId")]
        public string UserId { get; set; }
        public KhetiApplicationUser User { get; set; }
        [Required]
        public DateTime RequestStartDate { get; set; }
        [Required]
        public DateTime RequestEndDate { get; set; }
        public DateTime? ActualRequestStartDate { get; set; }
        public DateTime? ActualRequestEndDate { get; set; }
        public string BookingStatus { get; set; }
        public string PaymentStatus { get; set; }
        public string RentStatus { get; set; }
        public string? DamageDescription { get; set; }
        public string? DamagedImageUrl { get; set; }
        public DateTime? CreatedDate { get; set; }
        public decimal? InitialAmountPaid { get; set; }
        public decimal? InitialTotalAmount { get; set; }
        public decimal? ActualTotalAmountWithoutFine { get; set; }
        public decimal? FineAmount { get; set; }
        public decimal? TotalAmountAfterFine { get; set; }
        public decimal? RemainingAmountToPayAfterFine { get; set; }
        public decimal? RemainingAmountPaid { get; set; }

        public decimal? TotalAmountPaid { get; set;}
        public ICollection<BookingComments> BookingComments { get; set; }

    }
}
