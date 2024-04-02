using Kheti.Data;
using Kheti.KhetiUtils;
using Kheti.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            
            if( catergory != null )
            {
                _db.Categories.Remove( catergory );
                _db.SaveChanges();
                TempData["delete"] = "Category Deleted";
                return RedirectToAction("CategoryList");
            }

            return NotFound();

        }

        public IActionResult UserList()
        {
            var users = _db.KhetiApplicationUsers.ToList(); 

            return View(users);
        }

        public IActionResult QueryList()
        {
            var queries = _db.QueryForms
                .Include(q => q.User)
                .ToList();

            return View(queries);
        }

        public IActionResult OrderList()
        {
            var order = _db.Orders.ToList();
            return View(order);
        }

        public IActionResult ReportList()
        {
            var reports = _db.Reports
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
            return View(reports);
        }


    }
}
