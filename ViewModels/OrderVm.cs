using Kheti.Models;

namespace Kheti.ViewModels
{
    public class OrderVm
    {
        public IEnumerable<Order> OrderList { get; set; }
        public Order Order { get; set; }
        public IEnumerable<OrderItem> OrderItems { get; set; }
    }
}
