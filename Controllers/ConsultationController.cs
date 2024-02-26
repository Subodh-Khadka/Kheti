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

namespace Kheti.Controllers
{
    [Authorize]
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

        [Authorize(Roles = "Seller,Expert")]
        public IActionResult QueryList()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            IEnumerable<QueryForm> queries;


            if (userRole == "Expert")
            {
                //retrieve the expertProfile for the current User
                var expertProfile = _db.ExpertProfiles.FirstOrDefault(u => u.UserId == userId);

                if (expertProfile != null)
                {
                    var expertise = expertProfile.FieldOfExpertise;

                    queries = _db.QueryForms
                        .OrderByDescending(q => q.UrgencyLevel == "High")
                        .Where(q => q.ProblemCategory == expertise).ToList();
                }
                else
                {
                    //if expertProfile is null
                    return RedirectToAction("Error", "Home");
                }

            }
            else
            //if user is of role:seller
            {
                queries = _db.QueryForms
                /*  .OrderByDescending(p => p.UrgencyLevel == "High")
                  .ThenByDescending(x => x.UrgencyLevel == "Medium")
                  .ThenByDescending(p => p.UrgencyLevel == "Low")*/
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.DateCreated).ToList();
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
                    queryForm.IsSolved = StaticDetail.QueryStatusPending;

                    queryForm.QueryStatus = KhetiUtils.StaticDetail.QueryStatusPending;
                }

                _db.QueryForms.Add(queryForm);
                _db.SaveChanges();

                TempData["success"] = "Query submitted successfully";

                /*return RedirectToAction(nameof(QueryList), "Consultation");*/
                return RedirectToAction("CreateQuery");
            }

            return View();
        }

        public IActionResult QueryDetails(int queryId)
        {
            var query = _db.QueryForms
        .OrderByDescending(q => q.DateCreated)
        .Include(q => q.QueryComments) // Include related comments
            .ThenInclude(c => c.User) // Optionally include user information for each comment
        .Include(q => q.User) // Optionally include user information for the query
        .FirstOrDefault(x => x.Id == queryId);

            // Retrieve past messages for the query
            var pastMessages = query.QueryComments.ToList();

            // Pass past messages to the view
            ViewBag.PastMessages = pastMessages;

                return View(query);
        }

        //queryComment post method
        [HttpPost]
        public IActionResult QueryDetails(int queryFormId, string commentText)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            var query = _db.QueryForms
                   .Include(query => query.QueryComments)
                .FirstOrDefault(q => q.Id == queryFormId);
            if (query == null)
            {
                return RedirectToAction("Error");
            }

            var isExpert = userRole == "Expert";

            if (userRole == "Expert")
            {
                var queryComment = new QueryComment
                {
                    QueryFormId = queryFormId,
                    UserId = userId,
                    CommentText = commentText,
                    DateCreated = DateTime.Now,
                    IsExpert = true,
                };

                query.QueryComments.Add(queryComment);
                _db.SaveChanges();
            }
            else
            {
                var queryComment = new QueryComment
                {
                    QueryFormId = queryFormId,
                    UserId = userId,
                    CommentText = commentText,
                    DateCreated = DateTime.Now,
                };

                query.QueryComments.Add(queryComment);
                _db.SaveChanges();

                //send message through signalR
                var user = isExpert ? "Expert" : "Seller";
                _hubContext.Clients.All.SendAsync("ReceiveMessage", user, commentText);
            }

            return RedirectToAction("QueryDetails", new { queryId = queryFormId });
        }

        [HttpPost]
        public IActionResult SendMessage(int queryFormId, string commentText)
        {            
            Console.WriteLine("SendMessage action method hit with queryFormId: " + queryFormId + " and commentText: " + commentText);

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            var query = _db.QueryForms
                .Include(q => q.QueryComments) 
                .FirstOrDefault(q => q.Id == queryFormId);

            if (query == null)
            {
                return RedirectToAction("Error");
            }

            var isExpert = userRole == "Expert";

            var message = new QueryComment
            {
                CommentText = commentText,
                DateCreated = DateTime.Now,
                QueryFormId = queryFormId,
                UserId = userId,
                IsExpert = isExpert
            };
            _db.QueryComments.Add(message);
            _db.SaveChanges();

            // Send message through SignalR
            var user = isExpert ? "Expert" : "Seller";
            _hubContext.Clients.All.SendAsync("ReceiveMessage", user, commentText);

            return RedirectToAction("QueryDetails", new { queryId = queryFormId });
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




    }
}
