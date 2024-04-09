using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Kheti.Models
{
    public class SellerProfile
    {

        [Key]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        [ValidateNever]
        public KhetiApplicationUser User { get; set; }
        [Required]
        public string? CompanyName { get; set; }
        public string? PanNo { get; set; }
        
        [ValidateNever]
        public string? CitizenShipImage { get; set; }

        public bool IsVerified { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
    }
}
