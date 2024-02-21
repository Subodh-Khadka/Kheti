using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kheti.Models
{
    public class QueryComment
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string CommentText { get; set; }
        [Required]        
        public DateTime? DateCreated { get; set; }
        public bool IsExpert { get; set; }
        [Required]
        public int QueryFormId { get; set; }
        [ForeignKey("QueryFormId")]
        public QueryForm Form { get; set; }
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public KhetiApplicationUser User { get; set; }


        //navigation properties for reply
        public QueryReply QueryReply { get; set; }
        

    }
}
