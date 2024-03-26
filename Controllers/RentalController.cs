﻿using Kheti.Data;
using Kheti.KhetiUtils;
using Kheti.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Kheti.Controllers
{
    public class RentalController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public RentalController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index(string searchInput)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            IEnumerable<Product> products;

            if (userRole == "Seller")
            {
                products = _db.Products
                    .Include(p => p.Category)
                    .Include(p => p.User)
                    .Where(p => p.UserId == userId && p.IsDeleted == false && p.Category.Name == "machinery" && p.RentalEquipment.RentalPricePerDay != null);
            }
            else
            {
                products = _db.Products
                    .Include(p => p.Category)
                    .Include(p => p.User)
                    .Where(p => p.IsDeleted == false && p.Category.Name == "machinery" && p.RentalEquipment.RentalPricePerDay != null);
            }

            if (!string.IsNullOrEmpty(searchInput))
            {
                var lowerCaseSearchInput = searchInput.ToLower();
                products = products.Where(p => p.ProductName.ToLower().Contains(lowerCaseSearchInput));
            }

            var sortedProducts = products.OrderByDescending(p => p.CreatedDate).ToList();
            return View(sortedProducts);
        }
        [Authorize(Roles = "Seller,Customer,Expert")]
        public IActionResult Details(Guid id)
        {
            var product = _db.Products
                .Include(p => p.Category)
                .Include(p => p.User)
                .Include(p => p.ProductComments)
                    .ThenInclude(c => c.User)
                .Include(p => p.ProductComments)
                    .ThenInclude(c => c.Replies)
                        .ThenInclude(r => r.User)
                .Include(p => p.Reviews)
                .ThenInclude(p => p.User)
                .Include(p => p.RentalEquipment)
                .FirstOrDefault(p => p.ProductId == id);

            Booking booking = new()
            {
                Product = product,
                ProductId = id
            };
            return View(booking);
        }

        [HttpPost]
        public IActionResult Details(Booking booking)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                booking.UserId = userId;
                booking.bookingStatus = StaticDetail.BookingStatusPending;
                booking.PaymentStatus = StaticDetail.PaymentStatusPending;
                booking.CreatedDate = DateTime.Now;


                _db.Add(booking);
                _db.SaveChanges();
                TempData["success"] = "Booking Request Sent successfully!";

                return RedirectToAction("Details");
            }
            catch (Exception ex)
            {
                TempData["error"] = "Booking Request Not Sent!";
                return RedirectToAction("Details");
            }

        }

        public IActionResult RequestList()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole == "Seller")
            {
                var requestList = _db.Bookings
                    .Where(b => b.Product.UserId == userId)
                    .Include(b => b.Product)
                    .Include(b => b.User)
                   .OrderByDescending(b => b.CreatedDate)
                    .ToList();
                return View(requestList);

            }
            else
            {
                var requestList = _db.Bookings
                   .Where(b => b.UserId == userId)
                   .Include(b => b.Product)
                   .Include(b => b.User)
                   .OrderByDescending(b => b.CreatedDate)
                   .ToList();
                return View(requestList);
            }
        }

        public IActionResult RequestDetail(Guid BookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole == "Customer")
            {
                var requestList = _db.Bookings
                    .Include(b => b.Product)
                    .Include(r => r.Product.RentalEquipment)
                    .Include(b => b.BookingComments)
                    .Include(b => b.User)
                    .ToList()
                    .FirstOrDefault(b => b.BookingId == BookingId && b.UserId == userId);

                var pastMessages = requestList.BookingComments.ToList();
                ViewBag.PastMessages = pastMessages;

                return View(requestList);
            }
            else if (userRole == "Seller")
            {
                var requestList = _db.Bookings
                    .Include(b => b.Product)
                     .Include(r => r.Product.RentalEquipment)
                    .Include(b => b.BookingComments)
                    .Include(b => b.User)
                    .ToList()
                    .FirstOrDefault(b => b.BookingId == BookingId && b.Product.UserId == userId);

                var pastMessages = requestList.BookingComments.ToList();
                ViewBag.PastMessages = pastMessages;

                return View(requestList);
            }
            else
            {
                var requestList = _db.Bookings
                   .Include(b => b.Product)
                   .Include(r => r.Product.RentalEquipment)
                   .Include(b => b.BookingComments)
                   .Include(b => b.User)
                   .ToList()
                   .FirstOrDefault(b => b.BookingId == BookingId);

                var pastMessages = requestList.BookingComments.ToList();
                ViewBag.PastMessages = pastMessages;

                return View(requestList);
            }
        }

        [Authorize(Roles = "Seller")]
        [HttpPost]
        public IActionResult MarkAsApproved(Guid bookingId)
        {
            var booking = _db.Bookings.FirstOrDefault(b => b.BookingId == bookingId);
            if (booking != null)
            {
                booking.bookingStatus = StaticDetail.BookingStatusApproved;
                _db.SaveChanges();

                TempData["success"] = "Booking Approved by Seller";
            }
            else
            {
                TempData["error"] = "Error marking booking as Approved.";
            }
            return RedirectToAction("RequestDetail", new { BookingId = bookingId});
        }

        [HttpPost]
        public async Task<IActionResult> Pay(Guid bookingId)
        {
            Guid rental_booking_Id = bookingId;
            string returnUrl = "https://localhost:7108/Order/BookingPaymentConfirmationPage";
            int totalAmountInPaisa = 1000;
            string paymentUrl = await Kheti.KhetiUtils.KhaltiPayment.InitiateBookingPayment(rental_booking_Id, totalAmountInPaisa, returnUrl);

            return Redirect(paymentUrl);
        }

        public IActionResult BookingPaymentConfirmationPage(string pidx, string status, string transaction_id,
           Guid purchase_order_id, string purchase_order_name, string total_amount)
        {
            if (status == "Completed")
            {
                var booking = _db.Bookings.FirstOrDefault(o => o.BookingId == purchase_order_id);
                booking.PaymentStatus = StaticDetail.PaymentStatusCompleted;
                booking.bookingStatus = StaticDetail.BookingStatusConfirmed;
                _db.Bookings.Update(booking);
                _db.SaveChangesAsync();
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

