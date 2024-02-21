using Kheti.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using Kheti.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Kheti.Controllers
{
    [Authorize]
    public class ConsultationController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ConsultationController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
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

                if(expertProfile != null)
                {
                    var expertise = expertProfile.FieldOfExpertise;

                    queries = _db.QueryForms
                        .OrderByDescending(q => q.UrgencyLevel == "High")
                        .Where(q => q.ProblemCategory == expertise).ToList();
                }
                else
                {
                    //if expertProfile is null
                    return RedirectToAction("Error","Home");
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

                    queryForm.QueryStatus = KhetiUtils.StaticDetail.QueryStatusPending;
                }

                _db.QueryForms.Add(queryForm);
                _db.SaveChanges();

                return RedirectToAction(nameof(QueryList), "Consultation");
            }

            return View();
        }

        public IActionResult QueryDetails(int queryId)
        {
            var query = _db.QueryForms
                .OrderByDescending(q => q.DateCreated)
                .Include(q => q.QueryComments)
                .Include(q => q.User)
                .FirstOrDefault(x => x.Id == queryId);

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
            if (query ==  null)
            {
                return RedirectToAction("Error");
            }

            if(userRole == "Expert")
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
            }                       

            return RedirectToAction("QueryDetails", new {queryId = queryFormId});
        }



    }
}
