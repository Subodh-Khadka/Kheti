using Kheti.Data;
using Kheti.KhetiUtils;
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
        public IActionResult OrderList(string pidx, string status)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            /*var orderList = _db.Orders.Include(o => o.User).OrderByDescending(o => o.OrderCreatedDate).ToList();*/

            OrderVm = new()
            {
                OrderList = _db.Orders.Where(u => u.UserId == userId)
                .OrderByDescending(u => u.PaymentStatus)
                .ThenByDescending(u => u.OrderCreatedDate)
                .ToList(),
                OrderItems = _db.OrderItems.Include(o => o.Product).ThenInclude(oi => oi.User).ToList(),
                
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
            var product = _db.Products
                .Include(p => p.User)
                .Include(p => p.ProductComments)
                .FirstOrDefault(p => p.ProductId == reviewVm.ProductId);

            var review = new Review
            {
                UserId = userId,
                Comment = reviewVm.Comment,
                Rating = reviewVm.Rating,
                DateReviewed = DateTime.Now,
                ProductId = reviewVm.ProductId,
            };

            product.Reviews.Add(review);
            _db.SaveChanges();
            TempData["success"] = "Review Added!";

            return RedirectToAction("OrderDetails", new { orderId = orderId });

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


        [HttpPost]
        public async Task<IActionResult> Pay(int orderId)
        {
            int purchaseOrderId = orderId;

            string returnUrl = "https://localhost:7108/Order/OrderPaymentConfirmationPage";
            int totalAmountInPaisa = 1000;
            string paymentUrl = await Kheti.KhetiUtils.KhaltiPayment.InitiateOrderPayment(purchaseOrderId, totalAmountInPaisa, returnUrl);
            return Redirect(paymentUrl);
        }

        public IActionResult OrderPaymentConfirmationPage(string pidx, string status, string transaction_id, 
            int purchase_order_id, string purchase_order_name, string total_amount)
        {
            if(status == "Completed")
            {
                var order = _db.Orders.FirstOrDefault(o => o.OrderId == purchase_order_id);
                order.PaymentStatus = StaticDetail.PaymentStatusCompleted;
                order.OrderStatus = StaticDetail.OrderStatusShipped;
                _db.Orders.Update(order);
                _db.SaveChanges();
            }

            ViewData["Pidx"] = pidx;
            ViewData["Status"] = status;
            ViewData["TransactionId"] = transaction_id;
            ViewData["PurchaseOrderId"] = purchase_order_id;
            ViewData["PurchaseOrderName"] = purchase_order_name;
            ViewData["TotalAmount"] = total_amount;

            return View();
        }


    }
}
