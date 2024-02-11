    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace Kheti.Models
    {
        public class ProductReply
        {
            [Key]
            public int Id { get; set; }
        
            [Required]
            public string ReplyText { get; set; }
       
            public DateTime ReplyDate { get; set; }
        
            public int ProductCommentId { get; set; }

            [ForeignKey("ProductCommentId")]
            public ProductComment ProductComment { get; set; }
        
            public string UserId { get; set; }

            [ForeignKey("UserId")]
            public KhetiApplicationUser User { get; set; }
        }
    }
