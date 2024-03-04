using Kheti.Data;
using Kheti.KhetiUtils;
using Kheti.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kheti.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
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
                return RedirectToAction("CategoryList");
            }
            return View();
        }

        public IActionResult EditCategory(Category category) 
        {
            if (ModelState.IsValid)
            {
            }
                return View();
        }

        public IActionResult UserList()
        {
            var users = _db.KhetiApplicationUsers.ToList(); 

            return View(users);
        }
    }
}
