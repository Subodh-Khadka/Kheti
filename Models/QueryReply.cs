using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kheti.Models
{
    public class QueryReply
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string ReplyText { get; set; }
        [Required]
        public DateTime DateCreated { get; set; }
        public int QueryCommentId { get; set; }
        [ForeignKey("QueryCommentId")]
        public QueryComment Comment { get; set; }
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public KhetiApplicationUser User { get; set; }

    }
}
