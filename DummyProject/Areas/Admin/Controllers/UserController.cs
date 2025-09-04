using DummyProject.DataStorage.Data;
using DummyProject.DataStorage.Repository.IRepository;
using DummyProject.Model;
using DummyProject.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DummyProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin)]
    public class UserController : Controller
    {
        
        private readonly IUnitOfWork _uniteOfWork;
        private readonly ApplicationDbContext _context;
        public UserController(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _uniteOfWork = unitOfWork;
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        #region APIs
        [HttpGet]
        public IActionResult GetAll()
        {
            var userList = _context.ApplicationUsers.ToList();//data-aspnetuser
            var roles = _context.Roles.ToList();//asproles
            var userRoles=_context.UserRoles.ToList();//aspnetuserroles
            foreach (var user in userList)
            {
                var roleId = userRoles.FirstOrDefault(u=>u.UserId==user.Id).RoleId;
                user.Role = roles.FirstOrDefault(r=>r.Id == roleId).Name;
                if (user.CompanyId==null)
                {
                    user.Company = new Company()
                    {
                        Name=""
                    };
                }
                if (user.CompanyId!=null)
                {
                    user.Company = new Company()
                    {
                        Name =_uniteOfWork.Company.Get(Convert.ToInt32(user.CompanyId)).Name
                    };
                }
            }
            //did this to block admin
            var adminUser = userList.FirstOrDefault(u => u.Role == SD.Role_Admin);//we featch hte admin user in here
            if (adminUser != null)// why we did it -'admin can't lock himself if he dose who'll manage the project
                userList.Remove(adminUser);
            return Json(new { data = userList });
        }
        [HttpPost]//use to submit data
        public IActionResult LockUnlock([FromBody] string id)//we will take oly id caus we only need to lock/unlock user
        {
            bool isLocked = false;
            var userInDb = _context.ApplicationUsers.FirstOrDefault(au=>au.Id==id);//find the id from database
            if (userInDb == null)
                return Json(new { success = false, message = "Somthing Went wrong While Locking/Unlocking Process" });
            if(userInDb!=null && userInDb.LockoutEnd>DateTime.Now)
            {
                userInDb.LockoutEnd = DateTime.Now;
                isLocked = false;
            }
            else
            {
                userInDb.LockoutEnd = DateTime.Now.AddYears(100);
                isLocked = true;
            }
            _context.SaveChanges();
            return Json(new { success = true ,message = isLocked == true ? "User Locked 🔒" : "User Unlocked 🔓" });
        }
        #endregion
    }
}
