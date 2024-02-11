using Kheti.Data;
using Kheti.Models;
using Kheti.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
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

        /*[Authorize(Roles = "Seller,Customer")]*/
        public IActionResult Index(string searchInput)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            IEnumerable<Product> products;

            if (userRole == "Seller")
            {
                // Filter products to include only those listed by the logged-in seller
                products = _db.Products
                    .Include(p => p.Category)
                    .Include(p => p.User)
                    .Where(p => p.UserId == userId);
            }
            else // Customer role
            {
                // Display all products
                products = _db.Products
                    .Include(p => p.Category)
                    .Include(p => p.User);
            }

            if (!string.IsNullOrEmpty(searchInput))
            {
                var lowerCaseSearchInput = searchInput.ToLower();
                products = products.Where(p => p.ProductName.ToLower().Contains(lowerCaseSearchInput));
            }

            var sortedProducts = products.OrderByDescending(p => p.CreatedDate).ToList();
            return View(sortedProducts);
        }

        public IActionResult Details(Guid id)
        {                      
            var product = _db.Products
                .Include(p => p.Category)
                .Include(p => p.User)
                .Include(p => p.ProductComments)
                    .ThenInclude(c => c.User) // Include the user who posted the comment
                .Include(p => p.ProductComments)
                    .ThenInclude(c => c.Replies) // Include replies for each comment
                        .ThenInclude(r => r.User) // Include the user who posted the reply
                .FirstOrDefault(p => p.ProductId == id);

            ShoppingCart cart = new()
            {
                Product = product,
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

            return RedirectToAction("Index", "Cart");
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
        [Authorize]
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

        public IActionResult SubmitProductComment(Guid productId, string commentText)
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            // Retrieve the product
            var product = _db.Products
                .Include(p => p.ProductComments.OrderByDescending(pc => pc.CommentDate)) // Retrieve comments in reverse order
                .Include(p => p.User)
                .FirstOrDefault(p => p.ProductId == productId);

            // Create a new comment
            var comment = new ProductComment
            {
                ProductId = productId,
                CommentText = commentText,
                UserId = userId,
                CommentDate = DateTime.Now,                
                };

            // Add the comment to the product
            product.ProductComments.Add(comment);

            // Save changes
            _db.SaveChanges();

            // Return the partial view with the updated comments section
            var updatedProduct = _db.Products
                .Include(p => p.ProductComments) // Include comments
                    .ThenInclude(p => p.User)
                    .Include(p => p.ProductComments)
                    .ThenInclude(c => c.Replies) // Include replies for each comment
                    .ThenInclude(r => r.User) // Include user for each reply
                .FirstOrDefault(p => p.ProductId == productId);

            return PartialView("_CommentsPartial", updatedProduct.ProductComments);
            
        }

        public IActionResult SubmitProductReply(int commentId, string replyText)
        {
            // Retrieve the comment
            var comment = _db.ProductComments.Include(c => c.Replies).FirstOrDefault(c => c.Id == commentId);

            // Create a new reply
            var reply = new ProductReply
            {
                ProductCommentId = commentId,
                ReplyText = replyText,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                ReplyDate = DateTime.Now
            };

            // Add the reply to the comment
            comment.Replies.Add(reply);

            // Save changes
            _db.SaveChanges();

            //Return the list of replies for the comment
            var replies = _db.ProductReplies
                .Where(r => r.ProductCommentId == commentId)
                .Include(r => r.User)
                .ToList();

            return PartialView("_RepliesPartial", replies);

        }

    }
}
