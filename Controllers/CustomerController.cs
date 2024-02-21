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
                products = _db.Products
                    .Include(p => p.Category)
                    .Include(p => p.User)
                    .Where(p => p.UserId == userId);
            }
            else 
            {
                
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

        [Authorize(Roles = "Seller,Customer")]
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
                .FirstOrDefault(p => p.ProductId == id);

            ShoppingCart cart = new()
            {
                Product = product,
                Quantity = 1,
                ProductId = id
            };

            if (TempData["AddedToCartMessage"] != null)
            {
                ViewBag.AddedToCartMessage = TempData["AddedToCartMessage"];
            }
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }
            if (TempData["AddedToFavorite"] != null)
            {
                ViewBag.AddedToFavorite = TempData["AddedToFavorite"];
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

            // Set TempData for successful addition of product to cart
            TempData["AddedToCartMessage"] = "Product added to cart!";

            return RedirectToAction("Details");
        }

        
        public IActionResult FavoriteListing()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var favorites = _db.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Product)
                .ToList();

            if (TempData["AddedToCartMessage"] != null)
            {
                ViewBag.AddedToCartMessage = TempData["AddedToCartMessage"];
            }
            return View(favorites);
        }

        [HttpPost]
        [Authorize]
        public IActionResult AddToFavorite(Guid productId)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            
            var existingFavorite = _db.Favorites.FirstOrDefault(f => f.UserId == userId && f.ProductId == productId);
            if (existingFavorite != null)
            {
               
                TempData["Message"] = "Product is already in favorites!";
                return RedirectToAction("Details", new { id = productId });
            }

            var newFavorite = new Favorite
            {
                UserId = userId,
                ProductId = productId,
                AddedDate = DateTime.Now
            };
            _db.Favorites.Add(newFavorite);
            _db.SaveChanges();

            TempData["AddedToFavorite"] = "Product added to favorites!";

            return RedirectToAction("Details", new { id = productId });          
        }

        public IActionResult AddToCartFromFavorites(Guid productId) 
        {
            var claimsIdenttiy = (ClaimsIdentity)User.Identity;
            var userId = claimsIdenttiy.FindFirst(ClaimTypes.NameIdentifier).Value;

            //check if the product already exists in cart or not
            var existingCartInDb = _db.ShoppingCarts.FirstOrDefault(c => c.ProductId == productId && c.UserId == userId);

            if(existingCartInDb != null)
            {
                //if exists, update the quantity
                existingCartInDb.Quantity++;
                _db.ShoppingCarts.Update(existingCartInDb);
            }
            else
            {
                ShoppingCart newCart = new ShoppingCart()
                {
                    ProductId = productId,
                    UserId = userId,
                    Quantity = 1,
                };

                _db.ShoppingCarts.Add(newCart);
            }

            _db.SaveChanges();

            TempData["AddedToCartMessage"] = "Product added to cart!";

            return RedirectToAction("FavoriteListing");
        }


        //Method for deleting the products from the favorites
        public IActionResult DeleteFromFavorite(Guid id)
        {
            var productsToDelete = _db.Favorites.FirstOrDefault(p => p.ProductId == id);

            if (productsToDelete != null)
            {
                _db.Favorites.Remove(productsToDelete);
                _db.SaveChanges();
            }

            return RedirectToAction("FavoriteListing");
        }

        public IActionResult SubmitProductComment(Guid productId, string commentText)
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            
            var product = _db.Products
                .Include(p => p.ProductComments.OrderByDescending(pc => pc.CommentDate)) 
                .Include(p => p.User)
                .FirstOrDefault(p => p.ProductId == productId);

            
            var comment = new ProductComment
            {
                ProductId = productId,
                CommentText = commentText,
                UserId = userId,
                CommentDate = DateTime.Now,
            };
            
            product.ProductComments.Add(comment);
            
            _db.SaveChanges();
            
            var updatedProduct = _db.Products
                .Include(p => p.ProductComments) 
                    .ThenInclude(p => p.User)
                    .Include(p => p.ProductComments)
                    .ThenInclude(c => c.Replies) 
                    .ThenInclude(r => r.User) 
                .FirstOrDefault(p => p.ProductId == productId);

            return PartialView("_CommentsPartial", updatedProduct.ProductComments);

        }

        public IActionResult SubmitProductReply(int commentId, string replyText)
        {            
            var comment = _db.ProductComments.Include(c => c.Replies).FirstOrDefault(c => c.Id == commentId);
            
            var reply = new ProductReply
            {
                ProductCommentId = commentId,
                ReplyText = replyText,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                ReplyDate = DateTime.Now
            };
            
            comment.Replies.Add(reply);
            
            _db.SaveChanges();
            
            var replies = _db.ProductReplies
                .Where(r => r.ProductCommentId == commentId)
                .Include(r => r.User)
                .ToList();
                
            return PartialView("_RepliesPartial", replies);

        }
    }
}
