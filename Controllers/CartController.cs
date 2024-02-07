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
        /*[BindProperty]*/
        public ShoppingCartVM ShoppingCartVm { get; set; }

        public CartController(ApplicationDbContext db)
        {
            _db = db;
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

            ShoppingCartVm = new()
            {
                ShoppingCartList = _db.ShoppingCarts
                    .Where(u => u.UserId == userId)
                    .Include(p => p.Product)
                    .ToList(),
                Order = new()
            };

            ShoppingCartVm.Order.User = _db.KhetiApplicationUsers.FirstOrDefault(u => u.Id == userId);

            ShoppingCartVm.Order.CustomerName = ShoppingCartVm.Order.User.FirstName + " " + ShoppingCartVm.Order.User.LastName;
            ShoppingCartVm.Order.Address = ShoppingCartVm.Order.User.Address;
            ShoppingCartVm.Order.phoneNumber = ShoppingCartVm.Order.User.PhoneNumber;
            ShoppingCartVm.Order.Address = ShoppingCartVm.Order.User.Address;

            foreach (var cart in ShoppingCartVm.ShoppingCartList)
            {
                ShoppingCartVm.Order.OrderTotal += (decimal)cart.Product.Price * cart.Quantity;
            }

            return View(ShoppingCartVm);

        }

        [HttpPost]
        [ActionName("CartSummary")]
        public IActionResult CartSummaryPOST(ShoppingCartVM shoppingCartVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVm = new ShoppingCartVM()
            {
                ShoppingCartList = _db.ShoppingCarts
                    .Where(u => u.UserId == userId)
                    .Include(p => p.Product)
                    .ToList(),
                Order = new Order()

            };

            shoppingCartVM.Order.OrderCreatedDate = DateTime.Now;
            shoppingCartVM.Order.UserId = userId;

            ShoppingCartVm.Order.User = _db.KhetiApplicationUsers.FirstOrDefault(u => u.Id == userId);

            ShoppingCartVm.Order.CustomerName = ShoppingCartVm.Order.User.FirstName + " " + ShoppingCartVm.Order.User.LastName;
            ShoppingCartVm.Order.Address = ShoppingCartVm.Order.User.Address;
            ShoppingCartVm.Order.phoneNumber = ShoppingCartVm.Order.User.PhoneNumber;
            ShoppingCartVm.Order.Address = ShoppingCartVm.Order.User.Address;
            shoppingCartVM.Order.OrderTotal = 0;


            foreach (var cart in ShoppingCartVm.ShoppingCartList)
            {
                ShoppingCartVm.Order.OrderTotal += (decimal)cart.Product.Price * cart.Quantity;
            }

            shoppingCartVM.Order.PaymentStatus = KhetiUtils.StaticDetail.PaymentStatusPending;
            shoppingCartVM.Order.OrderStatus = KhetiUtils.StaticDetail.OrderStatusPending;


            _db.Orders.Add(shoppingCartVM.Order);
            _db.SaveChanges();

            foreach (var cart in ShoppingCartVm.ShoppingCartList)
            {
                OrderItem orderItem = new OrderItem()
                {
                    ProductId = cart.ProductId,
                    OrderId = shoppingCartVM.Order.OrderId,
                    Price = (decimal)cart.Product.Price,
                    Quantity = cart.Quantity,
                };
                _db.OrderItems.Add(orderItem);
                _db.SaveChanges();
            }
            var shoppingCartItems = _db.ShoppingCarts
           .Where(u => u.UserId == userId)
           .ToList();

            // Remove all items from the shopping cart
            _db.ShoppingCarts.RemoveRange(shoppingCartItems);
            _db.SaveChanges();
            // Clear the fields by creating a new empty ShoppingCartVM object
            ShoppingCartVm = new ShoppingCartVM()
            {
                ShoppingCartList = new List<ShoppingCart>(), // Clear the shopping cart list
                Order = new Order() // Create a new empty order
            };

            return RedirectToAction(nameof(OrderConformation), new { orderId = shoppingCartVM.Order.OrderId });
            /*return View(ShoppingCartVm);*/

        }

        public IActionResult OrderConformation(int orderID)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var shoppingCartItems = _db.ShoppingCarts
            .Where(u => u.UserId == userId)
            .ToList();

            // Remove all items from the shopping cart
            _db.ShoppingCarts.RemoveRange(shoppingCartItems);
            // Clear the fields by creating a new empty ShoppingCartVM object
            ShoppingCartVm = new ShoppingCartVM()
            {
                ShoppingCartList = new List<ShoppingCart>(), // Clear the shopping cart list
                Order = new Order() // Create a new empty order
            };
            return View(orderID);
        }        


    }
}
