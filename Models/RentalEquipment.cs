using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kheti.Models
{
    public class RentalEquipment
    {
        [Key]
        public Guid Id { get; set; }
        [ForeignKey("ProductId")]
        public Guid ProductId { get; set; }
        public Product Product { get; set; }

        [Required]
        public int RentalDuration { get; set; }
        [Required]
        public decimal RentalPricePerHour { get; set; }
        [Required]
        public decimal RentalPricePerDay { get; set; }
        [Required]
        public  bool IsAvailable{ get; set; }
        
        public string? TermsAndCondition { get; set; }
        [Required]
        public DateTime AvailabilityStartDate { get; set; }
        [Required]
        public DateTime AvailabilityEndDate { get; set; }
        [Required]
        public string Location {  get; set; }

        public decimal? DepositAmount { get; set; }
        
    }
}
