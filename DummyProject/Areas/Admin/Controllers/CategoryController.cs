using DummyProject.DataStorage.Repository.IRepository;
using DummyProject.Model;
using DummyProject.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DummyProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin + "," + SD.Role_Employee)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        #region APIs
        [HttpGet]
        public IActionResult GetAll()
        {
            var CategoryList = _unitOfWork.Category.GetAll();
            return Json(new {data=CategoryList});
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var userInCategory = _unitOfWork.Category.Get(id);
            if (userInCategory == null)
                return Json(new { success = false, message = "Somthing Went Wrong" });
            _unitOfWork.Category.Remove(userInCategory);
            _unitOfWork.save();
            return Json(new { success = true, message = "Data Deleted!!" });
        }
        #endregion
        public IActionResult Upsert(int? id)
        {
            Category category = new Category();
            if (id == null) return View(category);
            category = _unitOfWork.Category.Get(id.GetValueOrDefault());
            if (category == null) return NotFound();
            return View(category);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Category category)
        {
            if (category == null) return NotFound();
            if (!ModelState.IsValid) return View(category);
            if (category.Id ==0) _unitOfWork.Category.Add(category);
            else _unitOfWork.Category.Update(category);
            _unitOfWork.save();
            return RedirectToAction("Index");
        }
    }
}
