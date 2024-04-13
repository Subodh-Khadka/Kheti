using Grpc.Core;
using Kheti.Data;
using Kheti.KhetiUtils;
using Kheti.Models;
using Kheti.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

namespace Kheti.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {

        private readonly ApplicationDbContext _db;
        public OrderVm OrderVm { get; set; }
        public AdminController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CategoryList()
        {
            var categories = _db.Categories.ToList();
            return View(categories);
        }

        public IActionResult CreateCategory()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                /* if(_db.Categories.Any(c => c.Name == category.Name){

                 }*/
                _db.Categories.Add(category);
                _db.SaveChanges();
                TempData["success"] = "Category Added!";

                return RedirectToAction("CategoryList");
            }
            return View();
        }

        [HttpGet]
        public IActionResult EditCategory(int id)
        {
            var category = _db.Categories.FirstOrDefault(c => c.Id == id);

            return View(category);
        }

        [HttpPost]
        public IActionResult EditCategory(Category category)
        {
            var currentCategory = _db.Categories.FirstOrDefault(x => x.Id == category.Id);

            if (currentCategory != null)
            {
                currentCategory.Name = category.Name;
                _db.Categories.Update(currentCategory);
                _db.SaveChanges();
                TempData["update"] = "Category Edited!";
                return RedirectToAction("CategoryList");
            }

            return NotFound();
        }

        public IActionResult DeleteCategory(int id)
        {
            var catergory = _db.Categories.FirstOrDefault(c => c.Id == id);

            if (catergory != null)
            {
                _db.Categories.Remove(catergory);
                _db.SaveChanges();
                TempData["delete"] = "Category Deleted";
                return RedirectToAction("CategoryList");
            }

            return NotFound();

        }

        public IActionResult UserList(string searchInput)
        {
            var users = _db.KhetiApplicationUsers.ToList();

            if (!string.IsNullOrEmpty(searchInput))
            {
                var lowerCaseSearchInput = searchInput.ToLower();
                users = _db.KhetiApplicationUsers.Where(u => u.FirstName.ToLower().Contains(lowerCaseSearchInput)).ToList();
            }

            return View(users);
        }

        public IActionResult EditUserInformation(string id)
        {
            //var userId = _db.KhetiApplicationUsers.FirstOrDefault(u => u.Id == id);

            if (id != null)
            {
                var user = _db.KhetiApplicationUsers.FirstOrDefault(u => u.Id == id);
                return View(user);

            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditUserInformation(string id, KhetiApplicationUser updatedUser)
        {
            if (id != updatedUser.Id)
            {
                return NotFound();
            }
            var currentUser = _db.KhetiApplicationUsers.FirstOrDefault(u => u.Id == id);

            if (currentUser != null)
            {
                currentUser.FirstName = updatedUser.FirstName;
                currentUser.LastName = updatedUser.LastName;
                currentUser.District = updatedUser.District;
                currentUser.Address = updatedUser.Address;
                currentUser.Email = updatedUser.Email;
                currentUser.PhoneNumber = updatedUser.PhoneNumber;
                currentUser.LocalAddress = updatedUser.LocalAddress;
                currentUser.province = updatedUser.province;
                currentUser.AdditionalPhoneNumber = updatedUser.AdditionalPhoneNumber;

                _db.SaveChanges();

                TempData["Success"] = "Information updated!";

                return RedirectToAction("EditUserInformation", new { id = id });
            }
            else
            { }
            {
                TempData["ErrorMessage"] = "User not found.";
            }

            TempData["ErrorMessage"] = "Concurrency error occurred.";
            return RedirectToAction("Index");
        }

        public IActionResult DeleteUser(string id)
        {
            //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = _db.KhetiApplicationUsers.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                user.IsDeleted = true;
            }

            _db.KhetiApplicationUsers.Update(user);
            _db.SaveChanges();

            TempData["delete"] = "User Deleted";
            return RedirectToAction("Userlist");
        }

        public IActionResult LockUser(string id)
        {
            //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = _db.KhetiApplicationUsers.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                user.LockoutEnd = DateTime.Now.AddYears(1000);
            }

            _db.SaveChanges();

            TempData["delete"] = "User has been locked!";
            return RedirectToAction("Userlist");
        }

        public IActionResult UnlockUser(string id)
        {
            //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = _db.KhetiApplicationUsers.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                user.LockoutEnd = DateTime.Now;
            }

            _db.SaveChanges();

            TempData["success"] = "User has been unlocked!";
            return RedirectToAction("Userlist");
        }

        public IActionResult QueryList(string status)
        {

            IQueryable<QueryForm> queries = _db.QueryForms.Include(U => U.User);


            if (!string.IsNullOrEmpty(status))
            {
                switch (status.ToLower())
                {
                    case "pending":
                        queries = queries.Where(o => o.QueryStatus == StaticDetail.QueryStatusPending);
                        break;
                    case "inProcess":
                        queries = queries.Where(o => o.QueryStatus == StaticDetail.QueryStatusInProcess);
                        break;
                    case "solved":
                        queries = queries.Where(o => o.QueryStatus == StaticDetail.QueryStatusSolved);
                        break;
                    default:
                        queries = queries;
                        break;
                }
            }

            return View(queries.ToList());
        }

        //public IActionResult OrderList(string status)
        //{
        //    IQueryable<Order> orders = _db.Orders.Include(U => U.User);


        //    if (!string.IsNullOrEmpty(status))
        //    {
        //        switch (status.ToLower())
        //        {
        //            case "pending":
        //                orders = orders.Where(o => o.OrderStatus == StaticDetail.OrderStatusPending);
        //                break;
        //            case "completed":
        //                orders = orders.Where(o => o.OrderStatus == StaticDetail.OrderStatusShipped);
        //                break;
        //            case "canceled":
        //                orders = orders.Where(o => o.OrderStatus == StaticDetail.OrderStatusCanceled);
        //                break;
        //            default:
        //                orders = orders;
        //                break;
        //        }
        //    }

        //    return View(orders.ToList());
        //}

        public IActionResult OrderList(string status)
        {
            IQueryable<Order> orders = _db.Orders.Include(o => o.User);

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
                OrderList = orders
                .OrderByDescending(u => u.PaymentStatus)
                .ThenByDescending(u => u.OrderCreatedDate)
                .ToList(),
                OrderItems = _db.OrderItems.Include(o => o.Product).ThenInclude(oi => oi.User)
                .Include(o => o.Product.Category)
                .ToList(),

            };
            ViewData["Status"] = status;
            return View(OrderVm);
        }


        public IActionResult OrderDetails(int orderId, string userId)
        {

            var orderItems = _db.OrderItems
                .Include(o => o.Order)
                .Include(o => o.Order.User)
                .Include(oi => oi.Product)
                .Where(oi => oi.OrderId == orderId && oi.Order.UserId == userId).ToList();

            var orderVm = new OrderVm
            {
                OrderItems = orderItems,
                Order = orderItems.FirstOrDefault()?.Order
            };

            return View(orderVm);
        }


        public IActionResult BookingList(string status)
        {
            IQueryable<Booking> bookings = _db.Bookings.
                Include(U => U.User).
                Include(p => p.Product)
                .ThenInclude(p => p.RentalEquipment);


            if (!string.IsNullOrEmpty(status))
            {
                switch (status.ToLower())
                {
                    case "pending":
                        bookings = bookings.Where(o => o.BookingStatus == StaticDetail.BookingStatusPending);
                        break;
                    case "approved":
                        bookings = bookings.Where(o => o.BookingStatus == StaticDetail.BookingStatusApproved);
                        break;
                    case "confirmed":
                        bookings = bookings.Where(o => o.BookingStatus == StaticDetail.BookingStatusConfirmed);
                        break;
                    case "completed":
                        bookings = bookings.Where(o => o.BookingStatus == StaticDetail.BookingStatusCompleted);
                        break;
                    case "canceled":
                        bookings = bookings.Where(o => o.BookingStatus == StaticDetail.BookingStatusPending);
                        break;
                    default:
                        bookings = bookings;
                        break;
                }
            }

            return View(bookings.ToList());
        }

        public IActionResult ReportList(string status)
        {
            IQueryable<Report> reports = _db.Reports;


            if (!string.IsNullOrEmpty(status))
            {
                switch (status.ToLower())
                {
                    case "pending":
                        reports = reports.Where(o => o.ReportStatus == StaticDetail.ReportStatusPending);
                        break;
                    case "completed":
                        reports = reports.Where(o => o.ReportStatus == StaticDetail.ReportStatusSolved);
                        break;
                    default:
                        reports = reports;
                        break;
                }
            }

            return View(reports.ToList());
        }

        public IActionResult PaymentList(string searchInput)
        {
            var paymentList = _db.Payments
                .Include(p => p.Order)
                
                .Include(p => p.Booking)
                .Include(p => p.User)
                
                .ToList();

            if (!string.IsNullOrEmpty(searchInput))
            {
                var lowerCaseSearchInput = searchInput.ToLower();
                paymentList = _db.Payments.Where(u => u.TransactionId.ToLower().Contains(lowerCaseSearchInput)).ToList();
            }

            return View(paymentList);
        }

        public IActionResult MarkReportAsSolved(Guid id)
        {
            var report = _db.Reports.FirstOrDefault(r => r.Id == id);

            if (report != null)
            {
                report.ReportStatus = StaticDetail.ReportStatusSolved;
            }
            _db.Reports.Update(report);
            _db.SaveChanges();

            TempData["success"] = "Status changes to solved!";
            return RedirectToAction("ReportList");
        }

        public IActionResult PaymentDetails(int? orderId, Guid bookingId)
        {
            if (orderId.HasValue)
            {
                // Load order details based on orderId
                var orderDetails = _db.Orders
                    .Include(o => o.User)
                    .FirstOrDefault(o => o.OrderId == orderId.Value);
                var orderItems = _db.OrderItems
                    .Include(p => p.Product)
                    .Include(p => p.Order)
                    .Where(o => o.OrderId == orderId.Value)
                    .ToList();

                var orderVm = new OrderVm
                {
                    OrderItems = orderItems,
                    Order = orderDetails,
                };

                return View(orderVm);
            }
            else if (bookingId != null)
            {
                // Load booking details based on userId
                var bookingDetails = _db.Bookings
                    .Include(b => b.Product)
                     .Include(r => r.Product.RentalEquipment)
                     .Include(b => b.Product)
                     .Include(r => r.Product.User)
                    .Include(b => b.User)
                    .FirstOrDefault(b => b.BookingId == bookingId);
                return View("BookingDetails", bookingDetails);  
            }
            else
            {
                return BadRequest();
            }
        }

    }
}
