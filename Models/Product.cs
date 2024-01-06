using Newtonsoft.Json;
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
        public decimal ? Price { get; set; }
        [Required]
        public string PrdouctImageUrl { get; set; }
        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        //Navigation property to access the related Category
        public Category Category { get; set; }
        [ForeignKey("User")]
        public string UserId {  get; set; }
        public KhetiApplicationUser User { get; set; }
    }
}
