using DummyProject.DataStorage.Repository.IRepository;
using DummyProject.Model;
using DummyProject.Model.ViewModels;
using DummyProject.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Stripe;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;

namespace DummyProject.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private static bool isEmailConfirmed = false;
        private readonly IEmailSender _emailSender;//iemailsender is interface used to send emails (created by microsoft
        private readonly UserManager<IdentityUser> _userManager;//both properties can be used in _userManager

        public CartController(IUnitOfWork unitOfWork,IEmailSender emailSender,UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _userManager = userManager;
        }
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public IActionResult Index()
        {
            var claimsIdentity=(ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);//user who is logged in!!
            if (claims==null)
            {
                ShoppingCartVM = new ShoppingCartVM()
                {
                    listCart=new List<ShopingCart>()
                };
                return View(ShoppingCartVM);
            }
            ShoppingCartVM  = new ShoppingCartVM() 
            { 
                listCart = _unitOfWork.ShoppingCart.GetAll(sc=>sc.ApplicationUserId==claims.Value,includeProp:"Product"),//to fetch another table data(includeprop created by me)
                OrderHeader =new OrderHeader()
            };
            ShoppingCartVM.OrderHeader.OrderTotal = 0;
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.FirstOrDefault(au => au.Id==claims.Value);//user info
            //bool price
            foreach(var list in ShoppingCartVM.listCart)
            {
                list.Price=SD.GetPriceBasedOnQuantity(list.Count,list.Product.Price,list.Product.Price50,list.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += list.Price * list.Count;
                //if we want to show 100 word in discription
                if(list.Product.Description.Length>=100)
                {
                    list.Product.Description=list.Product.Description.Substring(0,99) + "...";
                }
            }
            // email confirm
            if (!isEmailConfirmed)
            {
                ViewBag.EmailMessage = "Email must be confirmed for authorized customer";
                ViewBag.EmailCSS = "text-danger";
                isEmailConfirmed = false;
            }
            else {
                ViewBag.EmailMessage = "Email send succesfully";
                ViewBag.EmailCSS = "text-success";

            }
                return View(ShoppingCartVM);
        }
        //go to email verification code---------------------------------------------------------------------------------------------------*    ,.|..
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPost()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitOfWork.ApplicationUser.FirstOrDefault(au => au.Id == claims.Value);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email Empty !!!");
            }
            else
            {

                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, code = code},
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
            }
            return RedirectToAction("Index");
        }
        //**************************************************************
        public IActionResult Plus(int id)
        {
            var cart =_unitOfWork.ShoppingCart.Get(id);
            cart.Count += 1;
            _unitOfWork.save();
            return RedirectToAction("Index");
        }
        public IActionResult Minus(int id)
        {
            var cart = _unitOfWork.ShoppingCart.Get(id);
            if (cart.Count == 1)
            {
                cart.Count = 1;
            } 
            else cart.Count -= 1;
            _unitOfWork.save();
            return RedirectToAction("Index");
        }
        public IActionResult Delete(int id)
        {
            var cart = _unitOfWork.ShoppingCart.Get(id);
            _unitOfWork.ShoppingCart.Remove(cart);
            _unitOfWork.save();
            var claimsIdentity = (ClaimsIdentity)User.Identity;//claims use to get more detailed info of user/ used in modern authentication 
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claims != null)
            {
                var Count = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value).ToList().Count;
                HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, Count);
            }
            return RedirectToAction("Index");
        }
        public IActionResult summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claims == null) return NotFound();
            ShoppingCartVM = new ShoppingCartVM()
            {
                listCart = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value,includeProp:"Product"),
                    OrderHeader = new OrderHeader()
            };
            ShoppingCartVM.OrderHeader.ApplicationUser =//assigine the value to shoopingcartvm.orderheader 
                _unitOfWork.ApplicationUser.FirstOrDefault(au=>au.Id == claims.Value);//finds singel id that match the condition
            foreach (var list in ShoppingCartVM.listCart)
            {
                list.Price = SD.GetPriceBasedOnQuantity(list.Count, list.Product.Price, list.Product.Price50, list.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (list.Price + list.Count);
                if (list.Product.Description.Length>100)//if discription letter is more than 100 then 
                {
                    list.Product.Description = list.Product.Description.Substring(0,99) + "...";//99 will be printed and after that ...
                }
            }
            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.Region = ShoppingCartVM.OrderHeader.ApplicationUser.Region;//taken from applicationuser to orderheader
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
           return View(ShoppingCartVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public IActionResult SummaryPost(string stripetoken)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claims == null) return NotFound();
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.FirstOrDefault(au => au.Id == claims.Value);//person loged in 
            ShoppingCartVM.listCart = _unitOfWork.ShoppingCart.GetAll(sc=>sc.ApplicationUserId == claims.Value,includeProp:"Product");//items in the cart
            ShoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusPending;
            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            ShoppingCartVM.OrderHeader.OrderDate= DateTime.Now;//current data
            ShoppingCartVM.OrderHeader.ApplicationUserId = claims.Value;//whose order
            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);//where to add
            _unitOfWork.save();
            foreach (var list in ShoppingCartVM.listCart)//for every item
            {
                list.Price = SD.GetPriceBasedOnQuantity(list.Count, list.Product.Price, list.Product.Price50, list.Product.Price100);//price will applied based on quantity




                OrderDetails orderDetails = new OrderDetails()//this for details (cart will be one but product can multipal so did this to get their details
                {
                    //what to save
                    ProductId = list.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = list.Price,
                    Count = list.Count,
                    //*********************
                };
                ShoppingCartVM.OrderHeader.OrderTotal += (list.Price * list.Count);
                _unitOfWork.OrderDetails.Add(orderDetails);
                _unitOfWork.save();



            }            
            _unitOfWork.ShoppingCart.RemoveRange(ShoppingCartVM.listCart);
            _unitOfWork.save();
            //session count_______________________
            HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, 0);
            //script Payment-------------------------------------------------------------------------------------
            if (stripetoken == null)
            {
                ShoppingCartVM.OrderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusApproved;
            }
            else
            {
                var options = new ChargeCreateOptions()
                {
                    Amount = Convert.ToInt32(ShoppingCartVM.OrderHeader.OrderTotal),//total amount
                    Currency = "usd",
                    Description = "Order Id : " + ShoppingCartVM.OrderHeader.Id.ToString(),
                    Source = stripetoken
                };
                var service = new ChargeService();
                Charge charge = service.Create(options);
                if (charge.BalanceTransactionId == null)//payment rejection
                    ShoppingCartVM.OrderHeader.PaymentStatus=SD.PaymentStatusRejected;
                else 
                    ShoppingCartVM.OrderHeader.TransactionId = charge.BalanceTransactionId;//refund in case return
                if(charge.Status.ToLower()=="succeeded")
                {
                    ShoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusApproved;
                    ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
                    ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
                }
                _unitOfWork.save();
            }
            return RedirectToAction("OrderConfirmation", "Cart", new { id = ShoppingCartVM.OrderHeader.Id });
        }
        public IActionResult OrderConfirmation(int id)
        {
            return View(id);
        }
    }
} 
