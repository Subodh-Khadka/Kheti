using Kheti.Data;
using Kheti.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Reflection;
using System.Security.Claims;


namespace Kheti.Controllers
{
    //controller for managing products
    [Authorize(Roles = "Seller")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        // Constructor to inject ApplicationDbContext and IWebHostEnvironment
        public ProductController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        // Action method to display a list of products
        public IActionResult Index()
        {
            //Retrieving the userId from the current user's claims
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = _db.KhetiApplicationUsers.FirstOrDefault(u => u.Id == userId);

            //if (user == null || user.SellerProfile == null || user.SellerProfile.IsVerified == false)
            //{
            //    TempData["delete"] = "User not verified!";
            //    return RedirectToAction("Index", "Home");
            //}

            //filtering the products based on the userId(seller)
            var products = _db.Products.Include(p => p.Category)
                .OrderByDescending(p => p.CreatedDate)
                .Where(p => p.UserId == userId && p.IsDeleted == false);
            return View(products);
        }


        //action method to display the product creation form
        public IActionResult Create()
        {
            // Retrieve the userId from the current user's Claims
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            //fetching the list of categories from the database
            var categories = _db.Categories.ToList();

            //Creating the selectList from the categories
            SelectList categoryList = new SelectList(categories, "Id", "Name");

            //Setting the category list in viewBag
            ViewBag.CategoryList = categoryList;

            return View();
        }


        // POST method to handle product creation
        [HttpPost]
        public IActionResult Create(Product product, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                // Retrieve the userId from the current user's Claims
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                product.UserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Handling image file upload
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
                    //set product image url
                    product.ProductImageUrl = Path.Combine("Images", "ProductImages", uniqueFileName);
                }

                // Check if the product belongs to the 'Machinery' category
                if (product.CategoryId == 3)
                {
                    // Create rental equipment object
                    RentalEquipment rentalEquipment = new RentalEquipment
                    {
                        RentalDuration = product.RentalEquipment.RentalDuration,
                        RentalPricePerHour = product.RentalEquipment.RentalPricePerHour,
                        RentalPricePerDay = product.RentalEquipment.RentalPricePerDay,
                        AvailabilityStartDate = product.RentalEquipment.AvailabilityStartDate,
                        AvailabilityEndDate = product.RentalEquipment.AvailabilityEndDate,
                        DepositAmount = product.RentalEquipment.DepositAmount,
                        IsAvailable = true,
                        ProductId = product.ProductId,
                        Location = product.RentalEquipment.Location,
                        TermsAndCondition = product.RentalEquipment.TermsAndCondition,
                        UserId = product.UserId,
                    };
                    product.RentalEquipment = rentalEquipment;
                }
                else
                {
                    product.RentalEquipment = null;
                }

                // Add the product to the database
                _db.Products.Add(product);
                _db.SaveChanges();
                TempData["success"] = "Product added successfully!";

                return RedirectToAction("Index");
            }

            // If ModelState is not valid, return to the same view with validation errors
            return View();
        }

        // Action method to display the product edit form
        public IActionResult Edit(Guid id)
        {
            var product = _db.Products
                .Include(c => c.Category)
                .Include(r => r.RentalEquipment)
                .FirstOrDefault(p => p.ProductId == id);
            var categories = _db.Categories.ToList();
            SelectList categoryList = new SelectList(categories, "Id", "Name");
            ViewBag.CategoryList = categoryList;

            return View(product);
        }


        // POST action method to handle product editing
        [HttpPost]
        public IActionResult Edit(Guid id, Product product, RentalEquipment rentalEquipment, IFormFile? imageFile)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            product.UserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            // Retrieve the existing product from the database
            var existingProduct = _db.Products
                .Include(p => p.RentalEquipment)
                .Include(p => p.Category)
                .FirstOrDefault(p => p.ProductId == id);

            // Check if the existing product exists
            if (existingProduct == null)
            {
                return NotFound();
            }

            // Update product properties
            existingProduct.ProductName = product.ProductName;
            existingProduct.ProductDescription = product.ProductDescription;
            existingProduct.Price = product.Price;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.Unit = product.Unit;
            existingProduct.IsInStock = product.IsInStock;

            if (existingProduct.Category.Name == "Machinery")
            {
                existingProduct.RentalEquipment.RentalDuration = rentalEquipment.RentalDuration;
                existingProduct.RentalEquipment.RentalPricePerDay = rentalEquipment.RentalPricePerDay;
                existingProduct.RentalEquipment.TermsAndCondition = rentalEquipment.TermsAndCondition;
                existingProduct.RentalEquipment.AvailabilityStartDate = rentalEquipment.AvailabilityStartDate;
                existingProduct.RentalEquipment.AvailabilityEndDate = rentalEquipment.AvailabilityEndDate;
                existingProduct.RentalEquipment.Location = rentalEquipment.Location;
                existingProduct.RentalEquipment.DepositAmount = rentalEquipment.DepositAmount;
            }

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

                existingProduct.ProductImageUrl = Path.Combine("Images", "ProductImages", uniqueFileName);
                _db.Products.Update(existingProduct);
                _db.SaveChanges();
            }
            else
            {
                if (existingProduct != null)
                {
                    product.ProductImageUrl = existingProduct.ProductImageUrl;
                }
            }
            _db.SaveChanges();
            TempData["success"] = "Product edited";
            return RedirectToAction("Index");
        }

        //method for deleting the product
        public IActionResult Delete(Guid id)
        {
            var productToDelete = _db.Products.FirstOrDefault(x => x.ProductId == id);

            var existingProductInCart = _db.ShoppingCarts.Where(x => x.ProductId == id).ToList();
            var existingProductInFavorite = _db.Favorites.Where(x => x.ProductId == id).ToList();

            //checking if the product exists in any cart or favorites and remove if it does
            if (existingProductInCart.Any() || existingProductInFavorite.Any())
            {
                foreach (var shoppingCart in existingProductInCart)
                {
                    _db.ShoppingCarts.Remove(shoppingCart);
                }


                foreach (var favorite in existingProductInFavorite)
                {

                    _db.Favorites.Remove(favorite);
                }
                _db.SaveChanges();
            }

            //soft delete the product
            productToDelete.IsDeleted = true;
            _db.SaveChanges();
            TempData["delete"] = "Product Deleted";
            return RedirectToAction("Index");
        }
    }
}
