using Kheti.Models;

namespace Kheti.ViewModels
{
    public class ProductViewModel
    {
        public Product Product { get; set; }
        public double AverageRating { get; set; }
        public IEnumerable<Category> Categories { get; set; }
    }
}
