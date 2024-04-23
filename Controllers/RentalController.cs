using Kheti.Data;
using Kheti.KhetiUtils;
using Kheti.Migrations;
using Kheti.Models;
using Kheti.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Kheti.Controllers
{
    [Authorize] // Requires authorization for accessing this controller
    public class RentalController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailSender _emailSender;

        // Constructor injection of required services
        public RentalController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment, IEmailSender emailSender)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
        }

        public IActionResult Index(string searchInput)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            IEnumerable<Product> products;

            // Filter rental products based on user role
            if (userRole == "Seller")
            {
                products = _db.Products // Retrieve rental products for sellers
                    .Include(p => p.Category)
                    .Include(p => p.User)
                    .Where(p => p.UserId == userId && p.IsDeleted == false && p.Category.Name == "machinery" && p.RentalEquipment.RentalPricePerDay != null);
            }
            else // Retrieve rental products for other users
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
            // Order products by created date and return the sorted list
            var sortedProducts = products.OrderByDescending(p => p.CreatedDate).ToList();
            return View(sortedProducts);
        }


        // Action method to display details of a rental product
        [Authorize(Roles = "Seller,Customer,Expert")]
        public IActionResult Details(Guid id)
        {
            // Retrieve detailed information about the rental product
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

            // Create a new booking object for the view
            Booking booking = new()
            {
                Product = product,
                ProductId = id
            };
            return View(booking);
        }

        // Action method to handle booking requests
        [HttpPost]
        public IActionResult Details(Booking booking, Guid productId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                var product = _db.Products.Include(p => p.RentalEquipment).FirstOrDefault(b => b.ProductId == productId);

                //user who is sends the booking request
                var user = _db.KhetiApplicationUsers.FirstOrDefault(u => u.Id == userId);
                var bookingUserEmail = user.Email;

                //user to whom the booking request is sent
                var sellerOwner = _db.KhetiApplicationUsers.FirstOrDefault(u => u.Id == product.UserId);
                var sellerOwnerEmail = sellerOwner.Email;

                // Populate booking object with necessary details
                booking.UserId = userId;
                booking.BookingStatus = StaticDetail.BookingStatusPending;
                booking.PaymentStatus = StaticDetail.PaymentStatusPending;
                booking.RentStatus = StaticDetail.RentStatusPending;
                booking.CreatedDate = DateTime.Now;
                booking.PricePerDay = product.RentalEquipment.RentalPricePerDay;

                _db.Add(booking);
                _db.SaveChanges();

                // Send email notification to seller/owner
                _emailSender.SendEmailAsync(sellerOwnerEmail, "Rental Request Recieved",
 $@"<html>
        <head>
            <style>
                .container {{
                    font-family: Arial;
                    max-width: 400px;
                    margin: 0 auto;
                    border: 1px solid #ddd;
                    border-radius: 5px;
                    background-color: #f9f9f9;
                }}
                .header {{
                    background-color: green;
                    color: white;
                    padding:10px;
                    border-radius: 5px 5px 0 0;
                }}
                .content {{
                    padding: 20px;
                }}
                .footer {{
                    background-color: #f0f0f0;
                    padding: 10px 20px;
                    border-radius: 0 0 5px 5px;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h2>Rental Request Recieved</h2>
                </div>
                <div class='content'>
                    <p> Rental request of id {booking.BookingId} has been received. Please check it out!</p>
                    <div class='Rental Details'>
                        <p><strong>Order ID:</strong> {booking.BookingId}</p>
                        <p><strong>Booking User Email: {bookingUserEmail}  </strong></p>
                        <p><strong>Booking User Name: {user.FirstName} {user.LastName}  </strong></p>
                        <p><strong>Product: {product.ProductName}</strong></p>
                        <p><strong>Start Date: {booking.RequestStartDate}</strong></p>
                        <p><strong>End Date: {booking.RequestEndDate}</strong></p>
                    </div>
                </div>
                <div class='footer'>
                    <p>If you have any questions, please contact our support team.</p>
                </div>
            </div>
        </body>
    </html>");
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
            var booking = _db.Bookings.Include(b => b.User).FirstOrDefault(o => o.BookingId == bookingId);

            Guid purchaseOrderId = bookingId;

            string returnUrl = "https://localhost:7108/Rental/BookingPaymentConfirmationPage";
            int totalAmountInPaisa = 1000;
            string purchase_order_name = booking.User.FirstName + " " + booking.User.LastName;
            string paymentUrl = await Kheti.KhetiUtils.KhaltiPayment.InitiateBookingPayment(purchaseOrderId, totalAmountInPaisa, returnUrl, purchase_order_name);

            return Redirect(paymentUrl);
        }

        public IActionResult BookingPaymentConfirmationPage(string pidx, string status, string transaction_id,
           Guid purchase_order_id, string purchase_order_name, string total_amount)
        {
            var booking = _db.Bookings.FirstOrDefault(o => o.BookingId == purchase_order_id);

            if (booking.PaymentStatus == StaticDetail.PaymentStatusPartialPaid)
            {
                TempData["suceess"] = "Partial Payment Completed!";
                return RedirectToAction("RequestList");
            }

            if (status == "Completed")
            {

                TimeSpan rentalPeriod = booking.RequestEndDate - booking.RequestStartDate;
                int numOfDays = rentalPeriod.Days;

                var numberOfDays = numOfDays >= 1 ? numOfDays : 1;

                var initialTotalAmount = numberOfDays * booking.PricePerDay;

                var initialAmountPaid = initialTotalAmount * 0.3m;

                booking.PaymentStatus = StaticDetail.PaymentStatusPartialPaid;
                booking.BookingStatus = StaticDetail.BookingStatusConfirmed;
                booking.InitialTotalAmount = initialTotalAmount;
                booking.InitialAmountPaid = initialAmountPaid;
                //if (decimal.TryParse(total_amount, out decimal initialTotalAmountPaid))
                //{
                //    booking.InitialAmountPaid = initialTotalAmountPaid;
                //}

                _db.Bookings.Update(booking);
                _db.SaveChanges();

                Payment payment = new Payment
                {
                    UserId = booking.UserId,
                    TransactionId = transaction_id,
                    PaymentMethod = StaticDetail.khaltiPayment,
                    Amount = (decimal)booking.InitialAmountPaid,
                    PaymentDate = DateTime.Now,
                    OrderId = null,
                    BookingId = purchase_order_id,

                };

                _db.Payments.Add(payment);
                _db.SaveChanges();

                ViewData["Pidx"] = pidx;
                ViewData["Status"] = status;
                ViewData["TransactionId"] = transaction_id;
                ViewData["PurchaseOrderId"] = purchase_order_id;
                ViewData["PurchaseOrderName"] = purchase_order_name;
                ViewData["TotalAmount"] = initialAmountPaid;
            }

            TempData["success"] = "Payment Completed";
            return View();

        }

        public async Task<IActionResult> RemainingPay(Guid bookingId, decimal remainingAmount)
        {
            var booking = _db.Bookings.Include(b => b.User).FirstOrDefault(o => o.BookingId == bookingId);

            decimal remain = remainingAmount;
            Guid purchaseOrderId = bookingId;
            string returnUrl = "https://localhost:7108/Rental/FinalBookingPaymentConfirmationPage";
            int totalAmountInPaisa = 1000;
            string purchase_order_name = booking.User.FirstName + " " + booking.User.LastName;
            string paymentUrl = await Kheti.KhetiUtils.KhaltiPayment.InitiateRemainingBokkingPayment(purchaseOrderId, totalAmountInPaisa, returnUrl, purchase_order_name);

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

                if (booking.PaymentStatus == StaticDetail.PaymentStatusCompleted)
                {
                    return View();
                }

                if (total_amount != null)
                {
                    booking.RemainingAmountPaid = booking.RemainingAmountToPayAfterFine;
                    booking.TotalAmountPaid = booking.InitialAmountPaid + booking.RemainingAmountPaid;
                }
                booking.PaymentStatus = StaticDetail.PaymentStatusCompleted;
                booking.BookingStatus = StaticDetail.BookingStatusCompleted;
                _db.Bookings.Update(booking);

                Payment payment = new Payment
                {
                    UserId = booking.UserId,
                    TransactionId = transaction_id,
                    PaymentMethod = StaticDetail.khaltiPayment,
                    Amount = (decimal)booking.RemainingAmountToPayAfterFine,
                    PaymentDate = DateTime.Now,
                    OrderId = null,
                    BookingId = purchase_order_id,

                };

                _db.Payments.Add(payment);
                _db.SaveChanges();

                ViewData["Pidx"] = pidx;
                ViewData["Status"] = status;
                ViewData["TransactionId"] = transaction_id;
                ViewData["PurchaseOrderId"] = purchase_order_id;
                ViewData["PurchaseOrderName"] = purchase_order_name;
                ViewData["TotalAmount"] = booking.TotalAmountPaid;
            }

            TempData["success"] = "Payment Completed";
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

                if (updateBooking.DamageAmount > 0)
                {
                    // Calculate damage amount
                    existingBooking.DamageAmount = updateBooking.DamageAmount;
                }

                //calculate the Final Total Amount
                TimeSpan actualRentalPeriod = existingBooking.ActualRequestEndDate.Value - existingBooking.ActualRequestStartDate.Value;
                var actualTotalAmount = (decimal)actualRentalPeriod.Days * existingBooking.PricePerDay;
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
            if (booking.DamageAmount != null && booking.DamageAmount != 0)  
            {
                remainingAmountToPay += (decimal)booking.DamageAmount;
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
                _db.SaveChanges();
            }

            TempData["success"] = "Rental Completed";
            return RedirectToAction("RequestList");
        }

        public IActionResult NoFine(Guid bookingId)
        {
            var booking = _db.Bookings.FirstOrDefault(o => o.BookingId == bookingId);

            if (bookingId != null)
            {
                booking.PaymentStatus = StaticDetail.PaymentStatusCompleted;
                booking.TotalAmountPaid = booking.InitialAmountPaid + booking.RemainingAmountPaid;
            }
            _db.SaveChanges(true);
            return RedirectToAction("RequestDetail", new { BookingId = bookingId });
        }
    }
}

