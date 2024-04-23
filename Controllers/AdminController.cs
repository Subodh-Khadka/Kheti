using Grpc.Core;
using Kheti.Data;
using Kheti.KhetiUtils;
using Kheti.Models;
using Kheti.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq;
using System.Security.Claims;

// Controller for administrative tasks, requires 'Admin' role for access
namespace Kheti.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {

        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public OrderVm OrderVm { get; set; }
        public AdminController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment, UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult CategoryList()  // Action method for displaying the list of categories
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

        //method for displaying the form to edit a category
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

        // Action method for displaying the list of products with search and filtering options
        public IActionResult ProductList(string searchInput, string status)
        {
            IQueryable<Product> products = _db.Products.Include(p => p.Category);

            // Apply search filter
            if (!string.IsNullOrEmpty(searchInput))
            {
                var lowerCaseSearchInput = searchInput.ToLower();
                products = _db.Products.Where(p => p.ProductName.ToLower().Contains(lowerCaseSearchInput))
                    .Include(p => p.Category);
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(status))
            {
                switch (status.ToLower())
                {
                    case "crop":
                        products = products.Where(o => o.Category.Name == "Crop");
                        break;
                    case "fertilizer":
                        products = products.Where(o => o.Category.Name == "Fertilizer");
                        break;
                    case "machinery":
                        products = products.Where(o => o.Category.Name == "Machinery");
                        break;
                    default:
                        products = products;
                        break;
                }
            }
            ViewData["Status"] = status;
            return View(products.ToList());
        }

        //retreive order detiails of specific order
        public IActionResult ProductDetails(Guid productId)
        {
            if (productId != null)
            {
                var product = _db.Products
                    .Include(p => p.RentalEquipment)
                    .Include(p => p.Category)
                    .FirstOrDefault(u => u.ProductId == productId);

                var categories = _db.Categories.ToList();
                SelectList categoryList = new SelectList(categories, "Id", "Name");
                ViewBag.CategoryList = categoryList;
                return View(product);
            }
            else
            {
                return NotFound();
            }
        }

        //edit product details
        [HttpPost]
        public IActionResult EditProductDetails(Guid productId, Product product, RentalEquipment rentalEquipment, IFormFile? imageFile)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            product.UserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var existingProduct = _db.Products
                .Include(p => p.RentalEquipment)
                .Include(p => p.Category)
                .FirstOrDefault(p => p.ProductId == productId);

            if (existingProduct == null)
            {
                return RedirectToAction("Error", "Home");
            }

            existingProduct.ProductName = product.ProductName;
            existingProduct.ProductDescription = product.ProductDescription;
            existingProduct.Price = product.Price;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.Unit = product.Unit;
            existingProduct.IsInStock = product.IsInStock;

            // Update rental equipment details if the product category is "Machinery"
            if (existingProduct.Category.Name == "Machinery")
            {
                existingProduct.RentalEquipment.RentalDuration = rentalEquipment.RentalDuration;
                existingProduct.RentalEquipment.RentalPricePerDay = rentalEquipment.RentalPricePerDay;
                existingProduct.RentalEquipment.TermsAndCondition = rentalEquipment.TermsAndCondition;
                existingProduct.RentalEquipment.AvailabilityStartDate = rentalEquipment.AvailabilityStartDate;
                existingProduct.RentalEquipment.AvailabilityEndDate = rentalEquipment.AvailabilityEndDate;
                existingProduct.RentalEquipment.Location = rentalEquipment.Location;
                existingProduct.RentalEquipment.DepositAmount = rentalEquipment.DepositAmount;
            }


            // Handle product image upload
            if (imageFile != null && imageFile.Length > 0)
            {
                //Save the image to wwwroot/Images/ProductImages
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "ProductImages");
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                var filePath = Path.Combine(imagePath, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    imageFile.CopyTo(stream);
                }

                existingProduct.ProductImageUrl = Path.Combine("Images", "ProductImages", uniqueFileName);
                _db.Products.Update(existingProduct);
                _db.SaveChanges();
            }
            else
            {
                if (existingProduct != null)
                {
                    product.ProductImageUrl = existingProduct.ProductImageUrl;
                }
            }

            TempData["success"] = "Product Edited";
            _db.SaveChanges();
            return RedirectToAction("ProductDetails", new { productId = product.ProductId });
        }

        public IActionResult DeleteProduct(Guid id, string status) // delete product method
        {
            var productToDelete = _db.Products.FirstOrDefault(x => x.ProductId == id);

            if (productToDelete == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var existingProductInCart = _db.ShoppingCarts.Where(x => x.ProductId == id).ToList();
            var existingProductInFavorite = _db.Favorites.Where(x => x.ProductId == id).ToList();

            if (existingProductInCart.Any() || existingProductInFavorite.Any())
            {
                foreach (var shoppingCart in existingProductInCart)
                {
                    _db.ShoppingCarts.Remove(shoppingCart);
                }


                foreach (var favorite in existingProductInFavorite)
                {

                    _db.Favorites.Remove(favorite);
                }
                _db.SaveChanges();
            }

            if (productToDelete.IsDeleted == false)
            {
                productToDelete.IsDeleted = true;
                TempData["delete"] = "Product Deleted";
            }
            else
            {
                productToDelete.IsDeleted = false;
                TempData["success"] = "Deleted Product Undone";
            }


            _db.SaveChanges();
            return RedirectToAction("ProductList", new { status = status });
        }

        //retrieve all users
        public async Task<IActionResult> UserList(string searchInput, string status)
        {
            IQueryable<KhetiApplicationUser> users = _db.KhetiApplicationUsers
                .Include(p => p.SellerProfile).Include(p => p.ExpertProfile)
                .OrderByDescending(o => o.RegistrationDate)
                ;

            if (!string.IsNullOrEmpty(searchInput))
            {
                var lowerCaseSearchInput = searchInput.ToLower();
                users = _db.KhetiApplicationUsers.Where(u => u.FirstName.ToLower().Contains(lowerCaseSearchInput) || u.Email.ToLower().Contains(lowerCaseSearchInput));
            }

            //filter based on status
            if (!string.IsNullOrEmpty(status))
            {
                switch (status.ToLower())
                {
                    case "customer":
                        var allUsers = await users.ToListAsync();
                        users = allUsers.Where(u => _userManager.IsInRoleAsync(u, "Customer").Result).AsQueryable();
                        break;
                    case "seller":
                        var allUsersSeller = await users.ToListAsync();
                        users = allUsersSeller.Where(u => _userManager.IsInRoleAsync(u, "Seller").Result).AsQueryable();
                        break;
                    case "expert":
                        var allUsersExpert = await users.ToListAsync();
                        users = allUsersExpert.Where(u => _userManager.IsInRoleAsync(u, "Expert").Result).AsQueryable();
                        break;
                    case "admin":
                        var allUsersAdmin = await users.ToListAsync();
                        users = allUsersAdmin.Where(u => _userManager.IsInRoleAsync(u, "Admin").Result).AsQueryable();
                        break;
                    default:
                        break;
                }
            }

            ViewData["Status"] = status;
            ViewBag.RoleStatus = status;
            return View(users.ToList());
        }

        //edit user information
        public IActionResult EditUserInformation(string id)
        {

            if (id != null)
            {
                var user = _db.KhetiApplicationUsers
                    .Include(u => u.SellerProfile)
                    .Include(u => u.ExpertProfile)
                    .FirstOrDefault(u => u.Id == id);
                return View(user);

            }
            else
            {
                return NotFound();
            }
        }

        //edit user details
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditUserInformation(string id, KhetiApplicationUser updatedUser)
        {
            if (id != updatedUser.Id)
            {
                return NotFound();
            }
            var currentUser = _db.KhetiApplicationUsers.FirstOrDefault(u => u.Id == id);

            if (currentUser != null) //update user details
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

            var user = _db.KhetiApplicationUsers.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                RedirectToAction("Error", "Home");
            }

            if (user.IsDeleted == false)
            {
                user.IsDeleted = true;
                TempData["delete"] = "User Deleted";
            }
            else
            {
                user.IsDeleted = false;
                TempData["success"] = "Deleted User Undone";
            }

            _db.KhetiApplicationUsers.Update(user);
            _db.SaveChanges();

            return RedirectToAction("Userlist");
        }

        //lock user 
        public IActionResult LockUser(string id)
        {
            var user = _db.KhetiApplicationUsers.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                user.LockoutEnd = DateTime.Now.AddYears(1000);
            }

            _db.SaveChanges();

            TempData["delete"] = "User has been locked!";
            return RedirectToAction("Userlist");
        }

        //unlock user
        public IActionResult UnlockUser(string id)
        {

            var user = _db.KhetiApplicationUsers.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                user.LockoutEnd = DateTime.Now;
            }

            _db.SaveChanges();

            TempData["success"] = "User has been unlocked!";
            return RedirectToAction("Userlist");
        }

        //seller verification method
        public IActionResult VerifySeller(string userId, string status)
        {
            var user = _db.KhetiApplicationUsers
                .Include(u => u.SellerProfile)
                .Include(u => u.ExpertProfile)
                .FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                return RedirectToAction("Error", "Home");
            }

            if (user.SellerProfile.IsVerified == false)
            {
                TempData["success"] = "Seller Verified";
                user.SellerProfile.IsVerified = true;
            }
            else if (user.SellerProfile.IsVerified == true)
            {
                TempData["delete"] = "Seller unverified";
                user.SellerProfile.IsVerified = false;
            }

            _db.KhetiApplicationUsers.Update(user);
            _db.SaveChanges();
            return RedirectToAction("UserList", new { status = status });
        }


        //retrieve all the queries of user
        public IActionResult QueryList(string status)
        {

            IQueryable<QueryForm> queries = _db.QueryForms
                .OrderByDescending(q => q.IsDeleted)
                .Include(U => U.User);

            if (!string.IsNullOrEmpty(status))
            {
                switch (status.ToLower())
                {
                    case "pending":
                        queries = queries.Where(o => o.QueryStatus == StaticDetail.QueryStatusPending);
                        break;
                    case "process":
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

            ViewData["Status"] = status;
            return View(queries.ToList());
        }

        public IActionResult DeleteQuery(int id, string status)
        {
            var query = _db.QueryForms.FirstOrDefault(q => q.Id == id);

            if (query == null)
            {
                return RedirectToAction("Error", "Home");
            }

            if (query.IsDeleted == false)
            {
                query.IsDeleted = true;
                TempData["delete"] = "Query Deleted";
            }
            else if (query.IsDeleted == true)
            {
                query.IsDeleted = false;
                TempData["success"] = "Query Restored";
            }
            else
            {
                query.IsDeleted = true;
                TempData["delete"] = "Query Deleted";
            }

            _db.QueryForms.Update(query);
            _db.SaveChanges();
            return RedirectToAction("QueryList", new { status = status });
        }

        // Action method for displaying the list of orders
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

        //method for viewing details of an order
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

        // Action method for displaying the list of booking
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

        // Action method for displaying the specififed booking details
        public IActionResult BookingDetails(Guid bookingId)
        {
            var bookingDetails = _db.Bookings
                 .Include(b => b.Product)
                  .Include(r => r.Product.RentalEquipment)
                  .Include(b => b.Product)
                  .Include(r => r.Product.User)
                 .Include(b => b.User)
                 .FirstOrDefault(b => b.BookingId == bookingId);

            return View(bookingDetails);
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

        //retrieve all the payments of users
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
                return RedirectToAction("BookingDetails", bookingDetails);
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }

        //for admin dashboard
        public IActionResult Index()
        {
            var adminDashboardViewModel = new AdminDashboardViewModel
            {
                TotalUsers = getTotalUsers(),
                TotalProducts = getTotalProducts(),
                TotalOrders = getTotalOrders(),
                TotalQueries = getTotalQueries(),
                RecentOrders = GetRecentOrders(5).Result,
                TotalRevenue = CalculateTotalRevenueOfOrder(), // Calculate total revenue of orders
                RecentQueries = GetRecentQueries(5).Result,
                PopularCategories = GetMostPopularCategories(3).Result,  // Get most popular categories
                RevenueDates = GetRevenueDates(), // Get revenue data for line chart
                RevenueAmounts = GetRevenueAmounts(),
                LastSoldProducts = GetLastSoldProducts(),  // Get last sold products
                TotalBookings = getTotalRentalBookings(), // Get total rental bookings and payments, and calculate total rental revenue
                TotalPayments = getTotalPayments(),
                TotalRentalRevenue = CalculateTotalRevenueOfRental(),
            };
            // Get total customers, sellers, and experts
            GetTotalUserCategories(out int totalCustomers, out int totalSellers, out int totalExperts);
            adminDashboardViewModel.TotalCustomers = totalCustomers;
            adminDashboardViewModel.TotalSellers = totalSellers;
            adminDashboardViewModel.TotalExperts = totalExperts;


            // Get total products in different categories
            GetTotalProductsCategories(out int totalCrops, out int totalFertilizer, out int totalMachinery);
            adminDashboardViewModel.TotalCropProduct = totalCrops;
            adminDashboardViewModel.TotalFertilizer = totalFertilizer;
            adminDashboardViewModel.TotalMachinery = totalMachinery;

            return View(adminDashboardViewModel);
        }

        //methods to show details in dashboard
        public int getTotalUsers()
        {
            return _db.KhetiApplicationUsers.Count();
        }

        public int getTotalProducts() // Method to get total products
        {
            return _db.Products.Count();
        }

        // Method to get total users in different categories (customers, sellers, experts)
        private void GetTotalUserCategories(out int totalCustomers, out int totalSellers, out int totalExperts)
        {
            totalCustomers = _db.KhetiApplicationUsers.Count(u => u.ExpertProfile == null && u.SellerProfile == null);
            totalSellers = _db.KhetiApplicationUsers.Count(u => u.SellerProfile != null);
            totalExperts = _db.KhetiApplicationUsers.Count(u => u.ExpertProfile != null);
        }

        // Method to get total products in different categories (crop, fertilizer, machinery)
        private void GetTotalProductsCategories(out int totalCrops, out int totalFertilizer, out int totalMachinery)
        {
            totalCrops = _db.Products.Count(u => u.Category.Name == "Crop");
            totalFertilizer = _db.Products.Count(u => u.Category.Name == "Fertilizer");
            totalMachinery = _db.Products.Count(u => u.Category.Name == "Machinery");
        }

        public int getTotalOrders()   // Method to get total orders
        {
            return _db.Orders.Count();
        }

        public int getTotalQueries() // Method to get total queries
        {
            return _db.QueryForms.Count();
        }

        public Task<List<Order>> GetRecentOrders(int count)  // Method to get recent orders
        {
            var recentOrders = _db.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderCreatedDate)
                .Take(count)
                .ToListAsync();
            return recentOrders;
        }

        public Task<List<QueryForm>> GetRecentQueries(int count)
        {
            var recentQueries = _db.QueryForms
                .Include(o => o.User)
                .OrderByDescending(o => o.DateCreated)
                .Take(count)
                .ToListAsync();
            return recentQueries;
        }

        public decimal CalculateTotalRevenueOfOrder() // Method to calculate total revenue of orders
        {
            return _db.Orders
                .Where(o => o.PaymentStatus == StaticDetail.PaymentStatusCompleted)
                .Sum(o => o.OrderTotal);
        }
        public async Task<List<Category>> GetMostPopularCategories(int count)
        {
            var popularCategories = await _db.Products
                .GroupBy(p => p.Category)
                .OrderByDescending(g => g.Count())
                .Take(count)
                .Select(g => g.Key)
                .ToListAsync();

            return popularCategories;
        }


        //for line chart
        private List<DateTime> GetRevenueDates()   // Method to get revenue dates for line chart
        {
            var thirtyDaysAgo = DateTime.Today.AddDays(-31);

            return _db.Orders
                .Where(o => o.PaymentStatus == StaticDetail.PaymentStatusCompleted && o.OrderCreatedDate >= thirtyDaysAgo)
                .OrderBy(o => o.OrderCreatedDate)
                .Select(o => o.OrderCreatedDate.Date)
                .Distinct()
                .ToList();
        }

        private List<decimal> GetRevenueAmounts()  // Method to get revenue amounts for line chart
        {
            var revenueAmounts = new List<decimal>();
            var thirtyDaysAgo = DateTime.Today.AddDays(-30);
            var dates = GetRevenueDates();

            foreach (var date in dates)
            {
                var totalRevenue = _db.Orders
                    .Where(o => o.PaymentStatus == StaticDetail.PaymentStatusCompleted && o.OrderCreatedDate.Date == date)
                    .Sum(o => o.OrderTotal);

                revenueAmounts.Add(totalRevenue);
            }

            return revenueAmounts;
        }

        private List<OrderItem> GetLastSoldProducts()  // Method to get last sold products
        {
            var lastSoldProducts = _db.OrderItems
                .Include(oi => oi.Product)
                .OrderByDescending(oi => oi.Order.OrderCreatedDate)
                .Take(12)
                .ToList();

            return lastSoldProducts;
        }

        //booking
        public int getTotalRentalBookings()
        {
            return _db.Bookings.Count();
        }

        public int getTotalPayments()
        {
            return _db.Payments.Count();
        }

        public decimal CalculateTotalRevenueOfRental() // Method to calculate total revenue of rental bookings
        {
            var amount = _db.Bookings
                .Where(o => o.PaymentStatus == StaticDetail.PaymentStatusCompleted)
                .Sum(o => o.TotalAmountPaid);
            decimal totalRevenue = (decimal)amount;

            return totalRevenue;
        }
    }
}
