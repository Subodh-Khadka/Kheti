using Kheti.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using Kheti.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Kheti.Controllers
{
    public class ConsultationController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ConsultationController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult CreateQuery()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var user = _db.KhetiApplicationUsers.FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                // Handle scenario where user is not found
                return RedirectToAction("Error");
            }
            /*
                        var userEmail = user.Email;
                        var userAddress = user.Address;*/

            //fetching and populating the categoryList
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
                    //save the image to rootPath
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
                }

                _db.QueryForms.Add(queryForm);
                _db.SaveChanges();

                return RedirectToAction(nameof(CreateQuery), "Consultation");
            }

            return View();
        }

    public IActionResult ViewQuery()
        {
            var allQuery = _db.QueryForms.ToList();

            return View();
        }
    }
}
