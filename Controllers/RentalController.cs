using Kheti.Data;
using Kheti.KhetiUtils;
using Kheti.Migrations;
using Kheti.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Kheti.Controllers
{
    [Authorize]
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
                booking.BookingStatus = StaticDetail.BookingStatusPending;
                booking.PaymentStatus = StaticDetail.PaymentStatusPending;
                booking.RentStatus = StaticDetail.RentStatusPending;
                booking.CreatedDate = DateTime.Now;


                _db.Add(booking);
                _db.SaveChanges();
                TempData["success"] = "Booking request sent!";

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
                booking.BookingStatus = StaticDetail.BookingStatusApproved;
                _db.SaveChanges();

                TempData["success"] = "Booking Approved by Seller";
            }
            else
            {
                TempData["error"] = "Error marking booking as Approved.";
            }
            return RedirectToAction("RequestDetail", new { BookingId = bookingId });
        }

        [HttpPost]
        public async Task<IActionResult> Pay(Guid bookingId)
        {
            Guid rental_booking_Id = bookingId;
            string returnUrl = "https://localhost:7108/Rental/BookingPaymentConfirmationPage";
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
                booking.PaymentStatus = StaticDetail.PaymentStatusPartialPaid;
                booking.BookingStatus = StaticDetail.BookingStatusConfirmed;
                if (decimal.TryParse(total_amount, out decimal initialTotalAmountPaid))
                {
                    booking.InitialAmountPaid = initialTotalAmountPaid;
                }

                _db.Bookings.Update(booking);
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

        public async Task<IActionResult> RemainingPay(Guid bookingId, decimal remainingAmount)
        {
            decimal remain = remainingAmount;
            Guid rental_booking_Id = bookingId;
            string returnUrl = "https://localhost:7108/Rental/FinalBookingPaymentConfirmationPage";
            int totalAmountInPaisa = 1000;
            string paymentUrl = await Kheti.KhetiUtils.KhaltiPayment.InitiateRemainingBokkingPayment(rental_booking_Id, totalAmountInPaisa, returnUrl);

            return Redirect(paymentUrl);
        }

        public IActionResult FinalBookingPaymentConfirmationPage(string pidx, string status, string transaction_id,
           Guid purchase_order_id, string purchase_order_name, string total_amount)
        {

            if (status == "Completed")
            {
                var booking = _db.Bookings.FirstOrDefault(o => o.BookingId == purchase_order_id);
              
                //if (decimal.TryParse(total_amount, out decimal remainningAmountPaid))
                //{
                //    booking.RemainingAmountPaid = remainningAmountPaid;
                //}
                if(total_amount  != null)
                {
                    booking.RemainingAmountPaid = booking.RemainingAmountToPayAfterFine;
                    booking.TotalAmountPaid = booking.InitialAmountPaid + booking.RemainingAmountPaid;
                    booking.RemainingAmountToPayAfterFine = 0;
                }
                booking.PaymentStatus = StaticDetail.PaymentStatusCompleted;
                booking.BookingStatus = StaticDetail.BookingStatusCompleted;
                _db.Bookings.Update(booking);
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

        public IActionResult MarkAsRentStarted(Guid bookingId)
        {
            var booking = _db.Bookings.FirstOrDefault(o => o.BookingId == bookingId);

            if (bookingId != null)
            {
                booking.RentStatus = StaticDetail.RentStatusInProcess;
                booking.ActualRequestStartDate = DateTime.Now;
                _db.SaveChanges();
            }

            TempData["success"] = "Rental Period Started";
            return RedirectToAction("RequestDetail", new { BookingId = bookingId });
        }

        public IActionResult InitiateReturnProcess(Guid bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            var booking = _db.Bookings
                .Include(p => p.Product)
                .ThenInclude(p => p.User)
                .Include(u => u.User)
                .FirstOrDefault(b => b.BookingId == bookingId);

            return View(booking);
        }

        [HttpPost]
        public IActionResult UpdateBookingInformation(Guid bookingId, Booking updateBooking, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                var existingBooking = _db.Bookings
                    .Include(p => p.Product)
                    .ThenInclude(u => u.User)
                    .Include(u => u.User)
                    .Include(p => p.Product.RentalEquipment)
                    .FirstOrDefault(b => b.BookingId == bookingId);
                if (existingBooking == null)
                {
                    return RedirectToAction("Error", "Home");
                }

                existingBooking.ActualRequestStartDate = updateBooking.ActualRequestStartDate;
                existingBooking.ActualRequestEndDate = updateBooking.ActualRequestEndDate;
                existingBooking.DamageDescription = updateBooking.DamageDescription;

                //calculate the initial Total Amount
                TimeSpan initialRentalPeriod = existingBooking.RequestEndDate - existingBooking.RequestStartDate;
                decimal initialTotalAmount = initialRentalPeriod.Days * existingBooking.Product.RentalEquipment.RentalPricePerDay;
                existingBooking.InitialTotalAmount = initialTotalAmount;


                //calculate the Final Total Amount
                TimeSpan actualRentalPeriod = existingBooking.ActualRequestEndDate.Value - existingBooking.ActualRequestStartDate.Value;
                decimal actualTotalAmount = actualRentalPeriod.Days * existingBooking.Product.RentalEquipment.RentalPricePerDay;
                decimal totalAmount = actualRentalPeriod.Days * existingBooking.Product.RentalEquipment.RentalPricePerDay;

                existingBooking.ActualTotalAmountWithoutFine = actualTotalAmount;


                //calcualte fine if returned late
                bool checkDate = existingBooking.ActualRequestEndDate > existingBooking.RequestEndDate;
                if (checkDate)
                {
                    TimeSpan lateReturnedPeriod = existingBooking.ActualRequestEndDate.Value - existingBooking.RequestEndDate;
                    decimal fineAmount = lateReturnedPeriod.Days * 500;
                    decimal remainingAmountToPay = existingBooking.TotalAmountAfterFine - existingBooking.InitialAmountPaid ?? 0;
                    totalAmount = totalAmount + fineAmount;
                    existingBooking.FineAmount = fineAmount;
                    existingBooking.RemainingAmountToPayAfterFine = remainingAmountToPay;
                    existingBooking.TotalAmountAfterFine = totalAmount;

                }
                else
                {
                    existingBooking.ActualTotalAmountWithoutFine = initialTotalAmount;
                }

                //existingBooking.TotalAmountAfterFine = totalAmount;

                if (imageFile != null && imageFile.Length > 0)
                {
                    //Save the image to wwwroot/Images/ProductImages
                    var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "BookingImages");
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    var filePath = Path.Combine(imagePath, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        imageFile.CopyTo(stream);
                    }

                    existingBooking.DamagedImageUrl = Path.Combine("Images", "BookingImages", uniqueFileName);
                }
                else
                {
                    updateBooking.DamagedImageUrl = existingBooking.DamagedImageUrl;
                }

                _db.Bookings.Update(existingBooking);
                _db.SaveChanges();
                TempData["success"] = "Booking Information Updated!";

                return RedirectToAction("FinalReturnSummaryBeforeRentCompletion", new { bookingId = bookingId });
            }

            // If ModelState is not valid, return to the same view with validation errors
            return View();
        }
        public IActionResult FinalReturnSummaryBeforeRentCompletion(Guid bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);


            var booking = _db.Bookings
                .Include(b => b.Product)
                .ThenInclude(b => b.User)
                .Include(b => b.Product.RentalEquipment)
                .Include(b => b.User)

                .FirstOrDefault(o => o.BookingId == bookingId);

            decimal remainingAmountToPay = 0;

            if (booking.FineAmount != null && booking.FineAmount != 0)
            {
                // Calculate remaining amount to pay if there's a fine
                remainingAmountToPay = booking.TotalAmountAfterFine - booking.InitialAmountPaid ?? 0;
            }

            ViewData["remainingAmount"] = remainingAmountToPay;
            booking.RemainingAmountToPayAfterFine = remainingAmountToPay;
            _db.Bookings.Update(booking);
            _db.SaveChanges(); 
            return View(booking);
        }

        public IActionResult FinalReturnSummaryAfterRentCompletion(Guid bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);


            var booking = _db.Bookings
                .Include(b => b.Product)
                .ThenInclude(b => b.User)
                .Include(b => b.Product.RentalEquipment)
                .Include(b => b.User)

                .FirstOrDefault(o => o.BookingId == bookingId);

            //decimal remainingAmountToPay = 0;

            //if (booking.FineAmount != null && booking.FineAmount != 0)
            //{
            //    // Calculate remaining amount to pay if there's a fine
            //    remainingAmountToPay = booking.TotalAmountAfterFine - booking.InitialAmountPaid ?? 0;
            //}

            //ViewData["remainingAmount"] = remainingAmountToPay;
            //booking.RemainingAmountToPayAfterFine = remainingAmountToPay;
            //_db.Bookings.Update(booking);
            //_db.SaveChanges();
            return View(booking);
        }

        public IActionResult MarkAsReturned(Guid bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);


            var booking = _db.Bookings
                .Include(b => b.Product)
                .ThenInclude(b => b.User)
                .Include(b => b.Product.RentalEquipment)
                .Include(b => b.User)

                .FirstOrDefault(o => o.BookingId == bookingId);

            if (booking != null)
            {
                booking.RentStatus = StaticDetail.RentStatusReturned;
                _db.Bookings.Update(booking);
                _db.SaveChanges();
            }

            TempData["success"] = "Rent Status changed to returned!";
            return RedirectToAction("RequestDetail", new { BookingId = bookingId });
        }
        public IActionResult MarkAsCompleted(Guid bookingId)
        {
            var booking = _db.Bookings.FirstOrDefault(o => o.BookingId == bookingId);

            if (bookingId != null)
            {
                booking.RentStatus = StaticDetail.RentStatusCompleted;
                booking.BookingStatus = StaticDetail.BookingStatusCompleted;
                //booking.ActualRequestStartDate = DateTime.Now;
                _db.SaveChanges();
            }

            TempData["success"] = "Rental Completed";
            //return RedirectToAction("RequestDetail", new { BookingId = bookingId });
            return RedirectToAction("RequestList");
        }

        public IActionResult NoFine(Guid bookingId)
        {
            var booking = _db.Bookings.FirstOrDefault(o => o.BookingId == bookingId);

            if(bookingId != null) 
            {
                booking.PaymentStatus = StaticDetail.PaymentStatusCompleted;
                booking.TotalAmountPaid = booking.InitialAmountPaid + booking.RemainingAmountPaid;
            }
            _db.SaveChanges(true);
            return RedirectToAction("RequestDetail", new {BookingId = bookingId});

        }
    }
}

