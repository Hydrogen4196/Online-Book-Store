using DummyProject.DataStorage.Repository.IRepository;
using DummyProject.Model;
using DummyProject.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DummyProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin + "," + SD.Role_Employee)]
    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CoverTypeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        #region APIs
        public IActionResult GetAll()
        {
            var getAllData = _unitOfWork.CoverType.GetAll();
            return Json(new { data = getAllData });
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var userInDb = _unitOfWork.CoverType.Get(id);
            if (userInDb == null)
                return Json(new { success = false, message = "Somthing Went Wrong" });
            _unitOfWork.CoverType.Remove(userInDb);
            _unitOfWork.save();
            return Json(new { success = true, message = "Data Deleted!!" });
        }
        #endregion
        public IActionResult Upsert(int? id)
        {
            CoverType coverType = new CoverType();
            if (id == null) return View(coverType);
            coverType = _unitOfWork.CoverType.Get(id.GetValueOrDefault());
            if (id == null) return NotFound();
            return View(coverType);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(CoverType coverType)
        {
            if (coverType == null) return NotFound();
            if (!ModelState.IsValid) return View(coverType);
            if (coverType.Id == 0)
                _unitOfWork.CoverType.Add(coverType);
            else 
                _unitOfWork.CoverType.Update(coverType);
            _unitOfWork.save();
            return RedirectToAction(nameof(Index));
        }
    }
}
