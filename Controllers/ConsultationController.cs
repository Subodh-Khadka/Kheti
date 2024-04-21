using Kheti.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using Kheti.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Kheti.Hubs;
using Kheti.KhetiUtils;
using Microsoft.Extensions.Options;

namespace Kheti.Controllers
{
    [Authorize(Roles = "Seller,Expert")]
    public class ConsultationController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHubContext<ChatHub> _hubContext;

        public ConsultationController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment, IHubContext<ChatHub> hubContext)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
            _hubContext = hubContext;
        }

        public IActionResult QueryList(string status)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var queryStatus = string.IsNullOrEmpty(status) ? "All" : status;

            IEnumerable<QueryForm> queries;

            //for user with role 'Expert'
            if (userRole == "Expert")
            {
                var expertProfile = _db.ExpertProfiles.FirstOrDefault(u => u.UserId == userId);

                if (expertProfile != null && queryStatus == "All")
                {
                    var expertise = expertProfile.FieldOfExpertise;

                    queries = _db.QueryForms
                        .OrderByDescending(q => q.UrgencyLevel == "High")

                        .Where(q => q.ProblemCategory == expertise).ToList();
                }

                else if (expertProfile != null && queryStatus != "all")
                {
                    var expertise = expertProfile.FieldOfExpertise;

                    if (queryStatus == StaticDetail.QueryStatusPending)
                    {
                        queries = _db.QueryForms
                       .OrderByDescending(q => q.UrgencyLevel == "High")
                       .Where(q => q.ProblemCategory == expertise
                       && q.QueryStatus == status).
                      ToList();
                    }

                    else
                    {
                        queries = _db.QueryForms
                       .OrderByDescending(q => q.UrgencyLevel == "High")
                       .Where(q => q.ProblemCategory == expertise
                       && q.QueryStatus == status
                       && q.SelectedExpertId == userId).ToList();
                    }
                }
                else
                {
                    return RedirectToAction("Error", "Home");
                }

            }
            else
            //if user is of role:seller
            {
                if (queryStatus == "All")
                {
                    queries = _db.QueryForms
                    .Where(p => p.UserId == userId)
                    .OrderByDescending(p => p.DateCreated).ToList();
                }
                else
                {
                    queries = _db.QueryForms
                    .Where(p => p.UserId == userId && p.QueryStatus == queryStatus)
                    .OrderByDescending(p => p.DateCreated).ToList();
                }
            }

            if (queryStatus != null)
            {
                TempData["defaultName"] = queryStatus;

            }
            return View(queries);

        }

        public IActionResult CreateQuery()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var user = _db.KhetiApplicationUsers.FirstOrDefault(u => u.Id == userId);

            ViewBag.CategoryList = new SelectList(_db.Categories, "Id", "Name");

            QueryForm form = new QueryForm

            {
                UserId = userId,
                Location = user.Address,
                Email = user.Email
            };

            return View(form);
        }

        [HttpPost]
        public IActionResult CreateQuery(QueryForm queryForm, IFormFile imageFile)
        {
            if (!ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                if (imageFile != null && imageFile.Length > 0)
                {
                    var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "QueryImages");
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    var filePath = Path.Combine(imagePath, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        imageFile.CopyTo(stream);
                    }

                    queryForm.UserId = userId;
                    queryForm.DateCreated = DateTime.Now;
                    queryForm.ImageUrl = Path.Combine("Images", "QueryImages", uniqueFileName);
                    queryForm.IsSelected = "false";
                    queryForm.IsSolved = "false";

                    queryForm.QueryStatus = KhetiUtils.StaticDetail.QueryStatusPending;
                }

                _db.QueryForms.Add(queryForm);
                _db.SaveChanges();

                TempData["success"] = "Query submitted successfully";

                return RedirectToAction("CreateQuery");
            }
            TempData["delete"] = "Query submission failed!";
            return View();
        }

        public IActionResult QueryDetails(int queryId)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            //for seller
            if (userRole == StaticDetail.SellerRole)
            {
                

                var query = _db.QueryForms
                .OrderByDescending(q => q.DateCreated)
                .Include(q => q.QueryComments)
                .ThenInclude(c => c.User)
                .Include(q => q.User)
                .FirstOrDefault(x => x.Id == queryId && x.UserId == userId);

                var seller = _db.KhetiApplicationUsers.FirstOrDefault(s => s.Id == query.UserId);
                var sellerProfile = seller.ProfilePictureURL;
                ViewData["SellerProfile"] =  sellerProfile;


                if (query == null)
                {
                    return Redirect($"/Identity/Account/AccessDenied");
                }

                var pastMessages = query.QueryComments.ToList();
                ViewBag.PastMessages = pastMessages;
                return View(query);
            }
            //for expert
            else
            {

                var query = _db.QueryForms
                .OrderByDescending(q => q.DateCreated)
                .Include(q => q.QueryComments)
                .ThenInclude(c => c.User)
                .Include(q => q.User)
                .FirstOrDefault(x => x.Id == queryId);

                var expert = _db.KhetiApplicationUsers.FirstOrDefault(s => s.Id == query.SelectedExpertId);
                var expertProfile = expert.ProfilePictureURL;
                ViewData["ExpertProfile"] = expertProfile;

                if (query == null)
                {
                    TempData["delete"] = "Not Available!";
                    return RedirectToAction("QueryList");
                }

                if (query.SelectedExpertId != userId)
                {
                    TempData["delete"] = "Not Available!";
                    return RedirectToAction("QueryList");
                }

                var pastMessages = query.QueryComments.ToList();

                ViewBag.PastMessages = pastMessages;

                return View(query);
            }
        }


        [Authorize(Roles = "Expert")]
        [HttpPost]
        public IActionResult MarkAsSolved(int queryFormId)
        {
            var query = _db.QueryForms.FirstOrDefault(q => q.Id == queryFormId);
            if (query != null)
            {
                query.QueryStatus = StaticDetail.QueryStatusSolved;
                _db.SaveChanges();

                TempData["success"] = "Marked query as solved";
            }
            else
            {
                TempData["error"] = "Error marking query as solved.";
            }
            return RedirectToAction("QueryList");
        }

        //for expert only
        [Authorize(Roles = "Expert")]
        public IActionResult MarkQueryAsSelected(int queryId)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var currentQuery = _db.QueryForms.FirstOrDefault(q => q.Id == queryId);

            if (currentQuery != null && currentQuery.IsSelected != "true")
            {
                currentQuery.IsSelected = "true";
                currentQuery.SelectedExpertId = userId;
                currentQuery.QueryStatus = StaticDetail.QueryStatusInProcess;
            }
            else
            {
                TempData["delete"] = "Query was selected already be else!";
                return RedirectToAction("QueryList");
            }

            _db.SaveChanges();

            return RedirectToAction("QueryList");
        }

    }
}
