using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kheti.Models
{
    public class Favorite
    {
        [Key]
        public int FavoriteId { get; set; }
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        [ValidateNever]
        public KhetiApplicationUser User { get; set; }
        [ForeignKey("productdId")]
        public Guid ProductId { get; set; }
        [ValidateNever]
        public Product Product { get; set; }

        public DateTime AddedDate { get; set; }

       

    }
}
