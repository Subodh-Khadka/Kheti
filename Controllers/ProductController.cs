﻿using Kheti.Data;
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
    [Authorize(Roles = "Seller,Admin")]
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

            var user = _db.KhetiApplicationUsers.FirstOrDefault(u => u.Id == userId);

            if (user == null || user.SellerProfile == null || user.SellerProfile.IsVerified == false)
            {
                TempData["delete"] = "User not verified!";
                return RedirectToAction("Index", "Home");
            }

            //filtering the products based on the userId
            var products = _db.Products.Include(p => p.Category)
                .Where(p => p.UserId == userId && p.IsDeleted == false);
            return View(products);
        }

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

                if (product.CategoryId == 3)
                {
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

                _db.Products.Add(product);
                _db.SaveChanges();
                TempData["success"] = "Product added successfully!";

                return RedirectToAction("Index");
            }

            // If ModelState is not valid, return to the same view with validation errors
            return View();
        }

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


        [HttpPost]
        public IActionResult Edit(Guid id, Product product, RentalEquipment rentalEquipment, IFormFile? imageFile)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            product.UserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var existingProduct = _db.Products
                .Include(p => p.RentalEquipment)
                .Include(p => p.Category)
                .FirstOrDefault(p => p.ProductId == id);

            if (existingProduct == null)
            {
                return NotFound();
            }

            existingProduct.ProductName = product.ProductName;
            existingProduct.ProductDescription = product.ProductDescription;
            existingProduct.Price = product.Price;
            existingProduct.CategoryId = product.CategoryId;

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
            return RedirectToAction("Index");
        }

        public IActionResult Delete(Guid id)
        {
            var productToDelete = _db.Products.FirstOrDefault(x => x.ProductId == id);

            var existingProductInCart = _db.ShoppingCarts.Where(x => x.ProductId == id).ToList();
            var existingProductInFavorite = _db.Favorites.Where(x => x.ProductId == id).ToList();

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

            productToDelete.IsDeleted = true;
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
