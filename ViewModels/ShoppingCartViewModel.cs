using Kheti.Models;

namespace Kheti.ViewModels
{
    public class ShoppingCartVM
    {
        public IEnumerable<ShoppingCart> ShoppingCartList { get; set; }
        public Order Orders { get; set; }
        public decimal Price { get; set; } 
        
    }
}
