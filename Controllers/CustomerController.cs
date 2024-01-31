using Kheti.Data;
using Kheti.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Kheti.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CustomerController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            var allProducts = _db.Products.Include(p => p.Category).ToList();
            return View(allProducts);
        }
        public IActionResult Details(Guid id)
        {            
            /* var claimsIdentity = (ClaimsIdentity)User.Identity;
             var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;*/

            ShoppingCart cart = new()
            {
                Product = _db.Products.Include(c => c.Category).FirstOrDefault(p => p.ProductId == id),
                Quantity = 1,
                ProductId = id                
            };
            return View(cart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.UserId = userId;
            
            ShoppingCart existingCartInDb = _db.ShoppingCarts.
                FirstOrDefault(u => u.UserId == userId && u.ProductId == shoppingCart.ProductId);

            if (existingCartInDb != null)
            {
                //if the cart already exists, update the quanity
                existingCartInDb.Quantity += shoppingCart.Quantity;
                _db.ShoppingCarts.Update(existingCartInDb);
            }
            else
            {
                //if no such cart exists, add the cart to Database
                _db.Add(shoppingCart);
            }
            
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }


    }
}
