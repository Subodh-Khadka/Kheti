using Grpc.Core;
using Kheti.Data;
using Kheti.KhetiUtils;
using Kheti.Models;
using Kheti.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Kheti.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _db;
        public OrderVm OrderVm { get; set; }

        public OrderController(ApplicationDbContext db)
        {
            _db = db;
        }

        //retrieve the order list of the customer
        public IActionResult OrderList(string status)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            IQueryable<Order> orders = _db.Orders.Where(u => u.UserId == userId);

            if (!string.IsNullOrEmpty(status))
            {
                switch (status.ToLower())
                {
                    case "pending":
                        orders = orders.Where(o => o.PaymentStatus == StaticDetail.PaymentStatusPending);
                        break;
                    case "completed":
                        orders = orders.Where(o => o.PaymentStatus == StaticDetail.PaymentStatusCompleted);
                        break;
                    default:
                        orders = orders;
                        break;
                }
            }
            OrderVm = new()
            {
                OrderList = orders.Where(u => u.UserId == userId)
                .OrderByDescending(u => u.PaymentStatus)
                .ThenByDescending(u => u.OrderCreatedDate)
                .ToList(),
                OrderItems = _db.OrderItems.Include(o => o.Product).ThenInclude(oi => oi.User).ToList(),

            };
            ViewData["Status"] = status;
            return View(OrderVm);
        }


        public IActionResult OrderDetails(int orderId)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var orderItems = _db.OrderItems
                .Include(o => o.Order)
                .ThenInclude(o => o.User)
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
            var order = _db.Orders.FirstOrDefault(o => o.OrderId == orderId);

            int purchaseOrderId = orderId;

            string returnUrl = "https://localhost:7108/Order/OrderPaymentConfirmationPage";
            int totalAmountInPaisa = 1000;
            string purchase_order_name = order.CustomerName;
            string paymentUrl = await Kheti.KhetiUtils.KhaltiPayment.InitiateOrderPayment(purchaseOrderId, totalAmountInPaisa,
                returnUrl, purchase_order_name);
            return Redirect(paymentUrl);
        }

        public IActionResult OrderPaymentConfirmationPage(string pidx, string status, string transaction_id,
            int purchase_order_id, string purchase_order_name, string total_amount)
        {
            var order = _db.Orders.FirstOrDefault(o => o.OrderId == purchase_order_id);

            if (order.PaymentStatus == StaticDetail.PaymentStatusCompleted) 
            {
                TempData["success"] = "Payment has been completed already!";
                return RedirectToAction("OrderList");
            }

            if (status == "Completed")
            {
                order.PaymentStatus = StaticDetail.PaymentStatusCompleted;
                order.OrderStatus = StaticDetail.OrderStatusShipped;
                _db.Orders.Update(order);

                //save payment in db
                Payment payment = new Payment
                {
                    UserId = order.UserId,
                    TransactionId = transaction_id,
                    PaymentMethod = StaticDetail.khaltiPayment,
                    Amount = order.OrderTotal,
                    PaymentDate = DateTime.Now,
                    OrderId = purchase_order_id,
                    BookingId = null,

                };

                _db.Payments.Add(payment);
                _db.SaveChanges();

                ViewData["Pidx"] = pidx;
                ViewData["Status"] = status;
                ViewData["TransactionId"] = transaction_id;
                ViewData["PurchaseOrderId"] = purchase_order_id;
                ViewData["PurchaseOrderName"] = purchase_order_name;
                ViewData["TotalAmount"] = total_amount;

            }
            TempData["success"] = "Payment Completed!";
            return View();
        }

        public IActionResult CancelOrder(int orderId, string status)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var order = _db.Orders.FirstOrDefault(o => o.OrderId == orderId);



            if (order != null)
            {
                order.OrderStatus = StaticDetail.OrderStatusCanceled;
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
            _db.Orders.Update(order);
            _db.SaveChanges();


            TempData["success"] = "Order Canceled";
            return RedirectToAction("OrderList", new { status = status });
        }

        public IActionResult OrderInvoice(int orderId)
        {
            var order = _db.Orders.FirstOrDefault(o => o.OrderId == orderId);

            if (order != null)
            {
                var orderItems = _db.OrderItems
            .Include(oi => oi.Product)
            .ThenInclude(p => p.User)
            .Where(oi => oi.OrderId == orderId)
            .ToList();

                var orderVm = new OrderVm()
                {
                    Order = order,
                    OrderItems = orderItems
                };

                var pdfData = GenerateOrderInvoicePdf(orderVm);

                var invoice = new Invoice
                {
                    OrderId = orderId,
                    CreatedAt = DateTime.Now,
                    PaymentStatus = order.PaymentStatus,
                };

                _db.Invoices.Add(invoice);
                _db.SaveChanges();

                TempData["success"] = "Invoice Generated";
                return File(pdfData.BinaryData, "application/pdf", $"Invoice_{orderId}.pdf");
            }
            else
            {
                TempData["error"] = "Some Error Occured";
                return RedirectToAction("OrderList");
            }
        }

        private IronPdf.PdfDocument GenerateOrderInvoicePdf(OrderVm orderVm)
        {
            var renderer = new IronPdf.ChromePdfRenderer();

            string html_order = $@"
             <html>
               <head>
                <link href=""https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css"" rel=""stylesheet"">
                <style>
                    .box
                        {{ 
                             box-shadow: 5px 5px 10px #888888;
                        }}
                </style>
                </head>
                <body>
       
                    <div class=""table table-bordered p-4"">
                      <h5 class=""text-success text-center"">Order Invoice</h5>
                      <p class=""fw-bold"">Order Id: {orderVm.Order.OrderId}</p>
                      <p class=""fw-bold"">Order Total: Rs: {orderVm.Order.OrderTotal}</p>
                      <p class=""fw-bold"">Address: {orderVm.Order.Address}</p>
                      <p class=""fw-bold"">Customer Name: {orderVm.Order.CustomerName}</p>
                     <h6 class=""text-success"">Order Items</h6>
                         <table class=""table table-bordered"">
                       <tr>
                        <th>Product Name</th>
                        <th>Quantity</th>
                        <th>Price (Rs)</th>
                        <th>Payment Status</th>
                       </tr>";

            foreach (var item in orderVm.OrderItems)
            {
                html_order += $@"
                        <tr>
                        <td>{item.Product.ProductName}</td>
                        <td>{item.Quantity}</td>
                        <td>{item.Price:R}</td>
                        <td>{item.Order.PaymentStatus:C}</td>
                       </tr>";
            }

            html_order += @"
                               </table>
                                </div>
                                </body>
                                </html>";

            var pdf = renderer.RenderHtmlAsPdf(html_order);
            return pdf;
        }
    }
}
