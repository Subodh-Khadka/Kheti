using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kheti.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }
        public int Rating { get; set; }
        [Required]
        public string Comment { get; set; }
        
        [ForeignKey ("ProductId")]
        public Guid ProductId { get; set; }
        public Product Product { get; set; }
        
        [ForeignKey("UserId")]
        public string UserId { get; set; }
        public KhetiApplicationUser User { get; set; }
        public DateTime DateReviewed { get; set; }
        public bool? IsDeleted { get; set; } = false;



    }
}
