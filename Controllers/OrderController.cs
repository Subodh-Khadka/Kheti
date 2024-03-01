using Kheti.Data;
using Kheti.Models;
using Kheti.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Kheti.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _db;
        public OrderVm OrderVm { get; set; }

        public OrderController(ApplicationDbContext db)
        {
            _db = db;
        }

        //retrieves the order list of the customer
        public IActionResult OrderList()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            /*var orderList = _db.Orders.Include(o => o.User).OrderByDescending(o => o.OrderCreatedDate).ToList();*/

            OrderVm = new()
            {
                OrderList = _db.Orders.Where(u => u.UserId == userId).ToList(),
                OrderItems = _db.OrderItems.Include(o => o.Product).ThenInclude(oi => oi.User).ToList(),
                /*Order = new()*/
            };
            return View(OrderVm);
        }


        public IActionResult OrderDetails(int orderId)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var orderItems = _db.OrderItems
                .Include(o => o.Order)
                .Include(oi => oi.Product)
                .ThenInclude(oi => oi.User)
                .Where(oi => oi.OrderId == orderId && oi.Order.UserId == userId).ToList();

            var orderVm = new OrderVm
            {
                OrderItems = orderItems,
                Order = orderItems.FirstOrDefault()?.Order

            };

            return View(orderVm);
        }

        [HttpPost]
        public IActionResult SaveReview(ReviewVm reviewVm, string orderId)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;            

            var review = new Review
            {
                UserId = userId,
                Comment = reviewVm.Comment,
                Rating = reviewVm.Rating,
                DateReviewed = DateTime.Now,
                ProductId = reviewVm.ProductId,
            };

            _db.Reviews.Add(review);
            _db.SaveChanges();

            return RedirectToAction("OrderDetails", new { orderId = orderId});
            
        }
        public IActionResult ReviewList()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var reviews = _db.Reviews 
                .Include(r => r.User)
                .Include(r => r.Product)
                .Where(r => r.UserId == userId).ToList();
            return View(reviews);
        }


        }
    }
