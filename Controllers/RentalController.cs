using Kheti.Data;
using Kheti.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Kheti.Controllers
{
    public class RentalController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public RentalController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index(string searchInput)
        {   
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            IEnumerable<Product> products;

            if (userRole == "Seller")
            {
                products = _db.Products
                    .Include(p => p.Category)
                    .Include(p => p.User)
                    .Where(p => p.UserId == userId && p.IsDeleted == false);
            }
            else
            {
                products = _db.Products
                    .Include(p => p.Category)
                    .Include(p => p.User)
                    .Where(p => p.IsDeleted == false && p.Category.Name == "machinery" && p.RentalEquipment.RentalPricePerDay != null);
            }

            if (!string.IsNullOrEmpty(searchInput))
            {
                var lowerCaseSearchInput = searchInput.ToLower();
                products = products.Where(p => p.ProductName.ToLower().Contains(lowerCaseSearchInput));
            }

            var sortedProducts = products.OrderByDescending(p => p.CreatedDate).ToList();
            return View(sortedProducts);
        }
        [Authorize(Roles = "Seller,Customer,Expert")]
        public IActionResult Details(Guid id)
        {
            var product = _db.Products
                .Include(p => p.Category)
                .Include(p => p.User)
                .Include(p => p.ProductComments)
                    .ThenInclude(c => c.User)
                .Include(p => p.ProductComments)
                    .ThenInclude(c => c.Replies)
                        .ThenInclude(r => r.User)
                .Include(p => p.Reviews)
                .ThenInclude(p => p.User)
                .Include(p => p.RentalEquipment)
                .FirstOrDefault(p => p.ProductId == id);         

            return View(product);
        }
    }
}
