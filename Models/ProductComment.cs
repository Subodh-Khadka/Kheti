using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kheti.Models
{
    public class ProductComment
    {
        [Key]
        public int Id { get; set; }
        public Guid ProductId { get; set; }
        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product Product { get; set; }
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        [ValidateNever]
        public KhetiApplicationUser User { get; set; }
        [Required]
        public string CommentText { get; set; }
        public DateTime CommentDate { get; set; }


        // Navigation property for replies
        public ICollection<ProductReply> Replies { get; set; }

    }
}
