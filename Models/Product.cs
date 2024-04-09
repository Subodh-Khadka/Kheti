    using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kheti.Models
{
    public class Product
    {
        [Key]
        public Guid ProductId { get; set; }

        [Required]
        public string ProductName { get; set; }

        [Required]
        public string ProductDescription { get; set; }

        [Required]
        public decimal? Price { get; set; }

        [Required]
        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        [Required]
        public string ProductImageUrl { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        [ForeignKey("User")]
        [ValidateNever]
        public string UserId { get; set; }
        public KhetiApplicationUser User { get; set; }

        // Navigation property for product comments
        public ICollection<ProductComment> ProductComments { get; set; }


        // Navigation property for replies
        public ICollection<ProductReply> Replies { get; set; }

        public bool? IsDeleted { get; set; } = false;

        public ICollection<Review> Reviews { get; set; } = new List<Review>();

        public RentalEquipment RentalEquipment { get; set; }
    }

}
