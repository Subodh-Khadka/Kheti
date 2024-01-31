using Kheti.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Kheti.Models
{
    public class ShoppingCart
    {
        [Key]
        public int ShoppingCartId { get; set; }

        public Guid ProductId { get; set; }
        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product Product { get; set; }

        public int Quantity { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        [ValidateNever]
        public KhetiApplicationUser User { get; set; }
    }
}