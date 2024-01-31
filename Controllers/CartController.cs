using Kheti.Data;
using Kheti.Models;
using Kheti.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Kheti.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ShoppingCartVM ShoppingCartVm { get; set; }

        public CartController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult New()
        {
            return View();
        }


        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVm = new()
            {
                ShoppingCartList = _db.ShoppingCarts.Where(u => u.UserId == userId).Include(p => p.Product).ToList(),
                Order = new()
                
            };

            foreach (var item in ShoppingCartVm.ShoppingCartList)
            {
                ShoppingCartVm.Order.OrderTotal += (decimal)item.Product.Price * item.Quantity;
            }

            return View(ShoppingCartVm);
        }

        private decimal CalculateOrderTotal(IEnumerable<ShoppingCart> shoppingCartItems)
        {
            decimal total = 0;

            foreach (var item in shoppingCartItems)
            {
                total += (decimal)item.Product.Price * item.Quantity;
            }

            return total;
        }

        public IActionResult AddCount(int itemId)
        {
            var existingCartFromDb = _db.ShoppingCarts.FirstOrDefault(u => u.ShoppingCartId == itemId);
            existingCartFromDb.Quantity += 1;
            _db.ShoppingCarts.Update(existingCartFromDb);
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult DecreaseCount(int itemId)
        {
            var existingCartFromDb = _db.ShoppingCarts.FirstOrDefault(u => u.ShoppingCartId == itemId);
            existingCartFromDb.Quantity -= 1;
            if (existingCartFromDb.Quantity < 1)
            {
                _db.Remove(existingCartFromDb);
            }
            _db.ShoppingCarts.Update(existingCartFromDb);
            _db.SaveChanges();

            return RedirectToAction("Index");
        }


        public IActionResult RemoveCart(int cartId)
        {
            var existingCartFromDb = _db.ShoppingCarts.FirstOrDefault(u => u.ShoppingCartId == cartId);

            _db.ShoppingCarts.Remove(existingCartFromDb);
            _db.SaveChanges();

            return RedirectToAction("Index");

        }

        public IActionResult CartSummary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVm = new ShoppingCartVM
            {
                ShoppingCartList = _db.ShoppingCarts
                    .Where(u => u.UserId == userId)
                    .Include(p => p.Product)
                    .ToList(),
                Order = new Order()
            };

            // Retrieve user from the database
            var user = _db.KhetiApplicationUsers.FirstOrDefault(u => u.Id == userId);

            if (user != null)
            {
                // Populate order properties from user
                ShoppingCartVm.Order.User = user;
                ShoppingCartVm.Order.CustomerName = user.FirstName + " " + user.LastName;
                ShoppingCartVm.Order.Address = user.Address;
                ShoppingCartVm.Order.phoneNumber = user.PhoneNumber;
            }

            // Calculate order total
            foreach (var item in ShoppingCartVm.ShoppingCartList)
            {
                ShoppingCartVm.Order.OrderTotal += (decimal)item.Product.Price * item.Quantity;
            }

            return View(ShoppingCartVm);
        }


        /* public IActionResult CartSummary() 
         {
             var claimsIdentity = (ClaimsIdentity)User.Identity;
             var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

             ShoppingCartVm = new()
             {
                 ShoppingCartList = _db.ShoppingCarts.Where(u => u.UserId == userId).Include(p => p.Product).ToList(),
                 Order = new()

             };

             ShoppingCartVm.Order.User = _db.KhetiApplicationUsers.FirstOrDefault(u => u.Id ==  userId);

             ShoppingCartVm.Order.CustomerName = ShoppingCartVm.Order.User.FirstName;
             ShoppingCartVm.Order.Address = ShoppingCartVm.Order.User.Address;

             foreach (var item in ShoppingCartVm.ShoppingCartList)
             {
                 ShoppingCartVm.Order.OrderTotal += (decimal)item.Product.Price * item.Quantity;
             }

             return View(ShoppingCartVm);

         }*/

    }
}
