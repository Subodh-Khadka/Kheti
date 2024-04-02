using Kheti.Data;
using Kheti.KhetiUtils;
using Kheti.Models;
using Kheti.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Kheti.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        public ShoppingCartVM ShoppingCartVm { get; set; }

        public CartController(ApplicationDbContext db, UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _db = db;
            _userManager = userManager;
            _emailSender = emailSender; 
        }

        [Authorize(Roles = "Customer")]
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;


            ShoppingCartVm = new()
            {
                ShoppingCartList = _db.ShoppingCarts.Where(u => u.UserId == userId).Include(p => p.Product).ToList(),
                Orders = new()

            };

            foreach (var item in ShoppingCartVm.ShoppingCartList)
            {
                ShoppingCartVm.Orders.OrderTotal += (decimal)item.Product.Price * item.Quantity;
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
                Orders = new()
            };

            ShoppingCartVm.Orders.User = _db.KhetiApplicationUsers.FirstOrDefault(u => u.Id == userId);

            ShoppingCartVm.Orders.CustomerName = ShoppingCartVm.Orders.User.FirstName + " " + ShoppingCartVm.Orders.User.LastName;
            ShoppingCartVm.Orders.Address = ShoppingCartVm.Orders.User.Address;
            ShoppingCartVm.Orders.phoneNumber = ShoppingCartVm.Orders.User.PhoneNumber;
            ShoppingCartVm.Orders.Address = ShoppingCartVm.Orders.User.Address;

            foreach (var cart in ShoppingCartVm.ShoppingCartList)
            {
                ShoppingCartVm.Orders.OrderTotal += (decimal)cart.Product.Price * cart.Quantity;
            }

            return View(ShoppingCartVm);

        }

       /* [HttpPost]
        [ActionName("CartSummary")]
        public IActionResult CartSummaryPOST(ShoppingCartVM shoppingCartVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVm = new()
            {
                ShoppingCartList = _db.ShoppingCarts
                    .Where(u => u.UserId == userId)
                    .Include(p => p.Product)
                    .ToList(),
                Orders = new()

            };

            ShoppingCartVm.Orders.OrderCreatedDate = DateTime.Now;
            ShoppingCartVm.Orders.UserId = userId;

            ShoppingCartVm.Orders.User = _db.KhetiApplicationUsers.FirstOrDefault(u => u.Id == userId);

            KhetiApplicationUser khetiApplicationUser = _db.KhetiApplicationUsers.FirstOrDefault(u => u.Id == userId);

            ShoppingCartVm.Orders.User = khetiApplicationUser;

            ShoppingCartVm.Orders.CustomerName = ShoppingCartVm.Orders.User.FirstName + " " + ShoppingCartVm.Orders.User.LastName;
            ShoppingCartVm.Orders.Address = ShoppingCartVm.Orders.User.Address;
            ShoppingCartVm.Orders.phoneNumber = ShoppingCartVm.Orders.User.PhoneNumber;
            ShoppingCartVm.Orders.OrderTotal = ShoppingCartVm.Orders.OrderTotal;
            ShoppingCartVm.Orders.Address = ShoppingCartVm.Orders.User.Address;


            foreach (var cart in ShoppingCartVm.ShoppingCartList)
            {
                ShoppingCartVm.Orders.OrderTotal += (decimal)cart.Product.Price * cart.Quantity;
            }

            ShoppingCartVm.Orders.PaymentStatus = KhetiUtils.StaticDetail.PaymentStatusPending;
            ShoppingCartVm.Orders.OrderStatus = KhetiUtils.StaticDetail.OrderStatusPending;


            _db.Orders.Add(ShoppingCartVm.Orders);
            _db.SaveChanges();

            foreach (var cart in ShoppingCartVm.ShoppingCartList)
            {
                OrderItem orderItem = new OrderItem()
                {
                    ProductId = cart.ProductId,
                    OrderId = shoppingCartVM.Orders.OrderId,
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
                ShoppingCartList = new List<ShoppingCart>(),
                Orders = new Order()
            };

            return RedirectToAction(nameof(OrderConformation), new { orderId = shoppingCartVM.Orders.OrderId });
        }*/

        [HttpPost]
        [ActionName("CartSummary")]
        public   IActionResult CartSummaryPost(ShoppingCartVM shoppingCartVM)
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                var user = _db.KhetiApplicationUsers.FirstOrDefault(u => u.Id == userId);
                var userEmail = user.Email;
/*
                if (shoppingCartVM == null || shoppingCartVM.Orders == null || shoppingCartVM.ShoppingCartList == null)
                {
                    return BadRequest("Invalid shopping cart data");
                }*/

                //populating ShoppingCartVm with shopping cart data
                ShoppingCartVm = new ShoppingCartVM()
                {
                    ShoppingCartList = _db.ShoppingCarts
                    .Where(u => u.UserId == userId)
                    .Include(p => p.Product)
                    .ToList(),
                    Orders = new Order()
                };

                ShoppingCartVm.Orders.OrderCreatedDate = DateTime.Now;
                ShoppingCartVm.Orders.UserId = userId;

                //populate order properties after getting the order properties
                var khetiApplicationUser = _db.KhetiApplicationUsers.FirstOrDefault(u => u.Id == userId);
                if (khetiApplicationUser != null)
                {
                    ShoppingCartVm.Orders.User = khetiApplicationUser;
                    ShoppingCartVm.Orders.CustomerName = $"{khetiApplicationUser.FirstName} {khetiApplicationUser.LastName}";
                    ShoppingCartVm.Orders.Address = khetiApplicationUser.Address;
                    ShoppingCartVm.Orders.phoneNumber = khetiApplicationUser.PhoneNumber;

                }

                ShoppingCartVm.Orders.OrderTotal = ShoppingCartVm.ShoppingCartList.Sum(cart =>
               (decimal)cart.Product.Price * cart.Quantity);

                ShoppingCartVm.Orders.PaymentStatus = KhetiUtils.StaticDetail.PaymentStatusPending;
                ShoppingCartVm.Orders.OrderStatus = KhetiUtils.StaticDetail.OrderStatusPending;

                _db.Orders.Add(ShoppingCartVm.Orders); ;
                _db.SaveChanges();

                //create order items and add them in the database
                foreach (var cart in ShoppingCartVm.ShoppingCartList)
                {
                    var orderItem = new OrderItem()
                    {
                        ProductId = cart.ProductId,
                        OrderId = ShoppingCartVm.Orders.OrderId,
                        Price = (decimal)cart.Product.Price,
                        Quantity = cart.Quantity,
                    };
                    _db.OrderItems.Add(orderItem);
                }
                _db.SaveChanges();

                //removing the items from the shopping cart
                _db.ShoppingCarts.RemoveRange(ShoppingCartVm.ShoppingCartList);
                _db.SaveChanges();

                //send email of order placed 
                _emailSender.SendEmailAsync(userEmail, $"Order Placed Successfully", $"Your order with ID {ShoppingCartVm.Orders.OrderId} has been placed successfully. Thank you for shopping with us!");

                //clear shoppingCartVm
                ShoppingCartVm = new ShoppingCartVM
                {
                    ShoppingCartList = new List<ShoppingCart>(),
                    Orders = new Order()
                };
                int orderId = shoppingCartVM.Orders.OrderId;
                int orderIds = ShoppingCartVm.Orders.OrderId;

                return RedirectToAction("OrderConformation", new { orderId = ShoppingCartVm.Orders.OrderId });
                //return RedirectToAction("OrderConformation", new {orderId = shoppingCartVM.Orders.OrderId});

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return View(shoppingCartVM);
        }

        public IActionResult OrderConformation(int orderId)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var shoppingCartItems = _db.ShoppingCarts
            .Where(u => u.UserId == userId)
            .ToList();

            ViewData["orderId"] = orderId;

            return View();
        }


    }
}
