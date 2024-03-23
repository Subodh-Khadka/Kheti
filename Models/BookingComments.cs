using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Kheti.Models
{
    public class BookingComments
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string CommentText { get; set; }
        [Required]
        public DateTime? DateCreated { get; set; }
        public bool IsSeller { get; set; }
        [Required]
        public Guid BookingId { get; set; }
        [ForeignKey("BookingId")]
        public Booking booking { get; set; }
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public KhetiApplicationUser User { get; set; }
        
    }
}
