using Kheti.Data;
using Kheti.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Reflection;
using System.Security.Claims;

namespace Kheti.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            //Retrieving the userId from the current user's claims
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            //filtering the products based on the userId
            var products = _db.Products.Include(p => p.Category)
                .Where(p => p.UserId == userId);
            return View(products);
        }

        public IActionResult Create()
        {
            // Retrieve the userId from the current user's Claims
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            ViewBag.UserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            //fetching the list of categories from the database
            var categories = _db.Categories.ToList();
            //Creating the selectList from the categories
            SelectList categoryList = new SelectList(categories, "Id", "Name");
            //Setting the category list in viewBag
            ViewBag.CategoryList = categoryList;

            return View();
        }

        [HttpPost]
        public IActionResult Create(Product product, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                // Retrieve the userId from the current user's Claims
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                product.UserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (imageFile != null && imageFile.Length > 0)
                {                    

                    //Save the image to wwwroot/Images/ProductImages
                    var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "ProductImages");
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    var filePath = Path.Combine(imagePath, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        imageFile.CopyTo(stream);
                    }

                    product.CreatedDate = DateTime.Now;
                    product.ProductImageUrl = Path.Combine("Images", "ProductImages", uniqueFileName);
                }

                _db.Products.Add(product);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }

            // If ModelState is not valid, return to the same view with validation errors
            return View();
        }

        public IActionResult Edit(Guid id)
        {
            var product = _db.Products.Include(c => c.Category).FirstOrDefault(p => p.ProductId == id);    

            var categories = _db.Categories.ToList();
            SelectList categoryList = new SelectList(categories, "Id", "Name");
            ViewBag.CategoryList = categoryList;
            
            return View(product);
        }

        [HttpPost]
        public IActionResult Edit(Guid id, Product product, IFormFile? imageFile)
        {
           
                // Retrieve the userId from the current user's Claims
                var claimsIdentity = (ClaimsIdentity)User.Identity;                
                product.UserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (imageFile != null && imageFile.Length > 0)
                {                   

                    // Example: Save the image to wwwroot/Images/ProductImages
                    var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "ProductImages");
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    var filePath = Path.Combine(imagePath, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        imageFile.CopyTo(stream);
                    }

                    product.ProductImageUrl = Path.Combine("Images", "ProductImages", uniqueFileName);
                }
                _db.Products.Update(product);
                _db.SaveChanges();
                return RedirectToAction("Index");                        
        }

        public IActionResult Delete(Guid id)
        {
            var product = _db.Products.FirstOrDefault(x => x.ProductId == id);
            _db.Products.Remove(product);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

       /* public IActionResult Favorite()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var favoriteList = _db.Favorites.Where(u => u.UserId == userId).Include(p => p.Product).ToList();


            return View(favoriteList);
        }

        [HttpPost]
        public IActionResult Favorite(Guid productId)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            //checking if the product is already been present in the user's favorite
            var existingFavorite = _db.Favorites.FirstOrDefault(f => f.UserId == userId && f.ProductId == productId);
            if (existingFavorite != null)
            {
                TempData["Message"] = "Product already exists in your wishlist!";
                return RedirectToAction("Details", "Customer", new { id = productId });
            }

            var favorite = new Favorite 
            {
                UserId = userId,
                ProductId = productId,
                AddedDate = DateTime.Now,
            };

            _db.Favorites.Add(favorite);
            _db.SaveChanges();


            return RedirectToAction("Favorite","Product");
        }*/
    }
}
