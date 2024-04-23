using Kheti.Data;
using Kheti.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kheti.Controllers
{
    //controller for managing product categories
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;

        //constructor to inject ApplicationDbContext
        public CategoryController (ApplicationDbContext db)
        {
            _db = db;
        }

        //method to display the list of categories
        [Authorize (Roles ="Admin")]
        public IActionResult Index()
        {
            var categories = _db.Categories.ToList();
            return View(categories);
        }

        //method to display the category creation form
        public IActionResult Create()
        {
            return View();
        }

        //post action method to handle category creation
        [HttpPost]
        public IActionResult Create(Category category)
        {
            if(ModelState.IsValid)
            {
                //add catrgory to the database
                _db.Categories.Add(category);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }

            //if model state is not valid, return the create view with errors
            TempData["warning"] = "Category not added"; 
            return View();
        }
    }
}
