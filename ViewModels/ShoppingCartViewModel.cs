using Kheti.Models;

namespace Kheti.ViewModels
{
    public class ShoppingCartVM
    {
        public IEnumerable<ShoppingCart> ShoppingCartList { get; set; }
        public decimal OrderTotal { get; set; }
    }
}
