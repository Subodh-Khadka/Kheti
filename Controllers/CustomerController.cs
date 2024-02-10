using Kheti.Data;
using Kheti.Models;
using Kheti.ViewModels;
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
            var allProducts = _db.Products.Include(p => p.Category).ToList().OrderByDescending(p => p.CreatedDate);
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
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }
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

            return RedirectToAction("Index","Cart");
        }

        public IActionResult FavoriteListing()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var favorites = _db.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Product)
                .ToList();

            return View("~/Views/Customer/FavoriteListing.cshtml", favorites);

        }




        [HttpPost]
        [Authorize] // Ensure user is logged in to add to favorites
        public IActionResult AddToFavorite(Guid productId)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            // Check if the product is already in favorites
            var existingFavorite = _db.Favorites.FirstOrDefault(f => f.UserId == userId && f.ProductId == productId);
            if (existingFavorite != null)
            {
                // Product already exists in favorites, handle accordingly
                // For example, display a message or redirect back to the details page
                TempData["Message"] = "Product is already in favorites!";
                return RedirectToAction("Details", new { id = productId });
            }

            // Add the product to favorites
            var newFavorite = new Favorite
            {
                UserId = userId,
                ProductId = productId,
                AddedDate = DateTime.Now
            };
            _db.Favorites.Add(newFavorite);
            _db.SaveChanges();

            // Redirect to the favorite listing page
            return RedirectToAction("FavoriteListing");
        }


            return RedirectToAction("Details", new { id = comment.ProductId, fragment = "comment-section" });

    }
}
