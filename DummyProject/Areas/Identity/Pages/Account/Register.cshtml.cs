// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using DummyProject.DataStorage.Repository.IRepository;
using DummyProject.Model;
using DummyProject.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace DummyProject.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
        }

        [BindProperty]
        public InputModel Input { get; set; }


        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        //controller code -----------


        public class InputModel
        {
        
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            //Rools
            [Required]
            public string Name { get; set; }
            [Display(Name = "Street Address")]
            public string StreetAddress { get; set; }
            public string City { get; set; }
            public string Region { get; set; }
            [Display(Name = "Postal Code")]
            public string PostalCode { get; set; }
            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; }
            [Display(Name = "Company")]
            public int? CompanyId { get; set; }//it can be null
            [ForeignKey("CompanyId")]
            public Company Company { get; set; }
            [NotMapped]
            public string Role { get; set; }
            //company
            public IEnumerable<SelectListItem> RoleList { get; set; }//selectlistitem use for dropdown list
            public IEnumerable<SelectListItem> CompanyList { get; set; }

        }

        //-------------------------------------------------------       
        public async Task OnGetAsync(string returnUrl = null) //if we access the page ongetasync
        {
            Input = new InputModel()
            {
                CompanyList=_unitOfWork.Company.GetAll().Select(cl=>new SelectListItem()
                {
                    Text = cl.Name,
                    Value= cl.Id.ToString()
                }),
                RoleList = _roleManager.Roles.Where(r=>r.Name!=SD.Role_Indvidual).
                Select(rl=>rl.Name).Select(rl=>new SelectListItem()//.Select(rl=>rl.Name) this is to  find only name of the perticular role assigned
                {
                    Text=rl,
                    Value=rl
                })
            };
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)//submit  POST
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                //var user = CreateUser();
                var user = new ApplicationUser()
                {
                    Name = Input.Name,
                    UserName = Input.Email,
                    Email = Input.Email,
                    PhoneNumber = Input.PhoneNumber,
                    StreetAddress = Input.StreetAddress,
                    City = Input.City,
                    Region = Input.Region,
                    PostalCode = Input.PostalCode,
                    CompanyId = Input.CompanyId,
                    Role = Input.Role,
                };

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                    //admin Role
                    if (!await _roleManager.RoleExistsAsync(SD.Role_Admin))
                    {
                       await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
                    }
                    if (!await _roleManager.RoleExistsAsync(SD.Role_Indvidual))
                    {
                       await _roleManager.CreateAsync(new IdentityRole(SD.Role_Indvidual));
                    }
                    if (!await _roleManager.RoleExistsAsync(SD.Role_Employee))
                    {
                       await _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee));
                    }
                    if (!await _roleManager.RoleExistsAsync(SD.Role_Company))
                    {
                       await _roleManager.CreateAsync(new IdentityRole(SD.Role_Company));
                    }
                    //Admin role
                  //await  _userManager.AddToRoleAsync(user, SD.Role_Admin);

                    if(Input.Role ==null && Input.CompanyId==null)//roleId/CompanyId is null, so it's a individual user
                    {
                       await _userManager.AddToRoleAsync(user, SD.Role_Indvidual);
                    }
                    else
                    {
                        if(Input.CompanyId>0)//if there is company id then this will happen 
                        {
                            await _userManager.AddToRoleAsync(user, SD.Role_Company);
                        }
                        else//if role this or other (in my case there is only 2 option so it will be role
                        {
                            await _userManager.AddToRoleAsync(user, Input.Role);
                        }
                    }


                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)//if registration confirm----- user will signIn
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else//else---
                    {
                        if(Input.Role==null && Input.CompanyId == null)// if there is no role/company(means indvidual user) it will signIn
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);//login dose
                            return LocalRedirect(returnUrl);
                        }
                        else//else---(means Admin (only admin can make user and company))
                        {
                            return RedirectToAction("Index","User",new {Area="Admin"});//return to index 
                        }
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}
