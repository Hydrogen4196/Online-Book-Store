using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DummyProject.Model.ViewModels;
using DummyProject.DataStorage.Repository.IRepository;
using DummyProject.Model;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using DummyProject.Utility;

namespace DummyProject.Areas.Customer.Controllers;

[Area("Customer")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;//claims use to get more detailed info of user/ used in modern authentication 
        var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
        if (claims != null)
        {
            var Count = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value).ToList().Count;
            HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, Count);
        }
        var productList = _unitOfWork.Product.GetAll(includeProp: "Category,CoverType");
        return View(productList);
    }
    #region API
    [HttpGet]
    public IActionResult GetSoldCounts()
    {
        var soldCounts = _unitOfWork.OrderDetails.GetAll().GroupBy(od => od.ProductId).Select(g => new { ProductId = g.Key, Count = g.Sum(od => od.Count) }).ToList();
        return Json(soldCounts);
    }
    #endregion

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    public IActionResult Details(int id)
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
        if (claims !=null )
        {
            var count = _unitOfWork.ShoppingCart.GetAll(sc=>sc.ApplicationUserId== claims.Value).ToList().Count;
            HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, count);
        }
        var productInDb = _unitOfWork.Product.FirstOrDefault
            (p=>p.Id==id,includeProp:"Category,CoverType");
        if (productInDb == null) return NotFound();
        var shopingCart = new ShopingCart()
        {
            Product = productInDb,
            ProductId = productInDb.Id
        };
        return View(shopingCart);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public IActionResult Details(ShopingCart shopingCart)
    {
        shopingCart.Id = 0;
        if(ModelState.IsValid)
        {
            var clasimIdentity = (ClaimsIdentity) (User.Identity);//claimidentity-- to access the identity of the user which one is loged in
            var claims = clasimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if(claims == null) return NotFound();
            shopingCart.ApplicationUserId = claims.Value;

            var shoppingCartInDb=_unitOfWork.ShoppingCart.FirstOrDefault//check if he user already have product added
                (sc =>sc.ApplicationUserId == claims.Value && sc.ProductId==shopingCart.ProductId);//(ShopingCart.ProductId) that product we wan
            if (shoppingCartInDb == null)
                _unitOfWork.ShoppingCart.Add(shopingCart);
            else shoppingCartInDb.Count += shopingCart.Count;
            _unitOfWork.save();
            return RedirectToAction("Index");
        }
        else
        {
            var productInDb = _unitOfWork.Product.FirstOrDefault
                (p => p.Id == shopingCart.Id, includeProp: "Category,CoverType");
            if (productInDb == null) return NotFound();
            var shopingCartEdit = new ShopingCart()
            {
                Product = productInDb,
                ProductId = productInDb.Id
            };
            return View(shopingCart);

        }            
    }
}
