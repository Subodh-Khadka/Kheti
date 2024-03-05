using Kheti.Data;
using Kheti.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kheti.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public UserController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }


        /*[HttpGet]*/
        public IActionResult EditInformation(string id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (id != null)
            {
                var user = _db.KhetiApplicationUsers.FirstOrDefault(u => u.Id == id);
                return View(user);

            }
            else
            {
                var user = _db.KhetiApplicationUsers.FirstOrDefault(u => u.Id == userId);
                return View(user);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string id, KhetiApplicationUser updatedUser)
        {
            if (id != updatedUser.Id)
            {
                return NotFound();
            }
            var currentUser = _db.KhetiApplicationUsers.FirstOrDefault(u => u.Id == id);

            if (currentUser != null)
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

                return RedirectToAction("EditInformation");
            }
            else
            {

            }
            {
                TempData["ErrorMessage"] = "User not found.";
            }

            TempData["ErrorMessage"] = "Concurrency error occurred.";
            return RedirectToAction("Index");
        }

        public IActionResult uploadProfile(string id, IFormFile profilePicture)
        {
            var currentUser = _db.KhetiApplicationUsers.FirstOrDefault(u => u.Id == id);

            if (profilePicture != null && profilePicture.Length > 0)
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "UserImages");
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + profilePicture.FileName;
                var filePath = Path.Combine(imagePath, uniqueFileName);
                Console.WriteLine(imagePath);
                Console.WriteLine(uniqueFileName);
                Console.WriteLine(filePath);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    profilePicture.CopyTo(stream);
                }
                currentUser.ProfilePictureURL = Path.Combine("Images", "UserImages", uniqueFileName);

                _db.SaveChanges();
                TempData["sucess"] = "Image Added";
                return RedirectToAction("EditInformation");
            }
            else
            {
                TempData["delete"] = "No Image Selected";
                return RedirectToAction("EditInformation");
            }
        }

        public IActionResult updateProfilePicture(string id, IFormFile profilePicture)
        {
            var currentUser = _db.KhetiApplicationUsers.FirstOrDefault(u => u.Id == id);

            if (profilePicture != null && profilePicture.Length > 0)
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "UserImages");
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + profilePicture.FileName;
                var filePath = Path.Combine(imagePath, uniqueFileName);
                Console.WriteLine(imagePath);
                Console.WriteLine(uniqueFileName);
                Console.WriteLine(filePath);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    profilePicture.CopyTo(stream);
                }
                currentUser.ProfilePictureURL = Path.Combine("Images", "UserImages", uniqueFileName);

                _db.SaveChanges();
                TempData["success"] = "Image Updated";
                return RedirectToAction("EditInformation");
            }
            else
            {
                TempData["delete"] = "No Image Selected";
                return RedirectToAction("EditInformation");
            }

        }

    }
}

