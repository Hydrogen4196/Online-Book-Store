using DummyProject.DataStorage.Repository.IRepository;
using DummyProject.Model.ViewModels;
using DummyProject.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using DummyProject.Utility;

namespace DummyProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin + "," + SD.Role_Employee)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }
        #region APIs
        public IActionResult GetAll()
        {
            return Json(new { data=_unitOfWork.Product.GetAll() });
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var webRootPath = _webHostEnvironment.WebRootPath;
            var productInDb = _unitOfWork.Product.Get(id);
            if (productInDb == null) return Json(new { success = false, message = "Somthing Went Wrong" });
            _unitOfWork.Product.Remove(productInDb);
            _unitOfWork.save();
            //image Delete
            var imagePath = Path.Combine(webRootPath,productInDb.ImageUrl.Trim('\\'));
            if(System.IO.File.Exists(imagePath))
                System.IO.File.Delete(imagePath);
            //***********
            return Json(new { success = true, message = "Data Deleted" });
        }
        #endregion
        public IActionResult Upsert(int? id)
        {
            //dropdown-------------------------------------with data
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategoryList = _unitOfWork.Category.GetAll().Select(cl => new SelectListItem()
                {
                    Text = cl.Name,
                    Value = cl.Id.ToString()
                }),
                CoverTypeList = _unitOfWork.CoverType.GetAll().Select(cl => new SelectListItem()
                {
                    Text = cl.Name,
                    Value = cl.Id.ToString()
                })
            };
            if (id == null) return View(productVM);
            productVM.Product = _unitOfWork.Product.Get(id.GetValueOrDefault());
            if (productVM.Product == null) return NotFound();
            return View(productVM);
            //-------------------------------------------------droupdown
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM)
        {
            if (ModelState.IsValid)                                                                       //vallidation check on server
            {
                var webRootPath = _webHostEnvironment.WebRootPath;                                        //path of wwRoot
                var files = HttpContext.Request.Form.Files;
                if (files.Count() > 0)                                                                    //if files more than 0(means file is selected)
                {
                    var fileName = Guid.NewGuid().ToString();                                            //genrate random name for file:-GUID(global unique identity)
                    var extension = Path.GetExtension(files[0].FileName);                                //select single file
                    var upload = Path.Combine(webRootPath, @"images\products");                           //path upto wwRoot in upload
                    if (productVM.Product.Id != 0)                                                       //if we have id (edit)
                    {
                        var imageExists = _unitOfWork.Product.Get(productVM.Product.Id).ImageUrl;        //if we are editing:-took oldImagePath and show it into ImageExist
                        productVM.Product.ImageUrl = imageExists;
                    }
                    if (productVM.Product.ImageUrl != null)                                              //delete old image code
                    {
                        var imagePath = Path.Combine(webRootPath, productVM.Product.ImageUrl.Trim('\\'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }
                    using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }
                    productVM.Product.ImageUrl = @"\images\products\" + fileName + extension;
                }
                else
                {
                    if (productVM.Product.Id != 0)
                    {
                        var imageExists = _unitOfWork.Product.Get(productVM.Product.Id).ImageUrl;
                        productVM.Product.ImageUrl = imageExists;                                        //person is updating but didn't update the image
                    }
                }
                if (productVM.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(productVM.Product);
                }
                else
                    _unitOfWork.Product.Update(productVM.Product);
                _unitOfWork.save();
                return RedirectToAction("Index");


            }
            else
            {
                productVM = new ProductVM()
                {
                    Product = new Product(),
                    CategoryList = _unitOfWork.Category.GetAll().Select(cl => new SelectListItem()
                    {
                        Text = cl.Name,
                        Value = cl.Id.ToString()
                    }),
                    CoverTypeList = _unitOfWork.CoverType.GetAll().Select(cl => new SelectListItem()
                    {
                        Text = cl.Name,
                        Value = cl.Id.ToString()
                    })
                };
                //---------------
                if (productVM.Product.Id != 0)//if there is an id 
                {
                    productVM.Product = _unitOfWork.Product.Get(productVM.Product.Id);//if there is an id. it take the info and display it to the boxes
                }
                //-----------
                return View(productVM);
            }
        }
    }
}
