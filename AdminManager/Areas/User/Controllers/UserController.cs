using AdminManager.Authentication;
using AdminManager.Data;
using AdminManager.Models;
using AdminManager.Models.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AdminManager.Areas.Customer.Controllers
{
    [Area("User")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
            IConfiguration configuration, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context, 
            IEmailSender emailSender, IHttpContextAccessor httpContextAccessor)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.roleManager = roleManager;
            _configuration = configuration;
            _context = context;
            _emailSender = emailSender;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task <IActionResult> Admin()
        {
            IEnumerable<DealerVM> dealers;
            dealers = (from user in _context.ApplicationUsers
                       join userrole in _context.UserRoles on user.Id equals userrole.UserId
                       join role in _context.Roles on userrole.RoleId equals role.Id
                       where (role.Name == "Dealer")
                       select (new DealerVM
                       {
                           UserName = user.UserName,
                           Role = role.Name,
                           Email = user.Email,
                           StatusType = user.StatusType.ToString()

                       }));
            return View(dealers);

        }


        public IActionResult AddRole()
        {
            var roles = roleManager.Roles.ToList();
            return View(roles);
        }
        public IActionResult AddAdmin()
        {
            return View();
        }

        public IActionResult AddRoles()
        {
            return View(new IdentityRole());
        }

        [HttpPost]
        public async Task<IActionResult> AddRole(IdentityRole role)
        {
            await roleManager.CreateAsync(role);
            return RedirectToAction("AddRole");
        }



        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Dealer()
        {
            return View();
        }

        public async Task <IActionResult> SuperAdmin()
        {

                IEnumerable<DealerVM> dealers;
                dealers = (from user in _context.ApplicationUsers
                           join userrole in _context.UserRoles on user.Id equals userrole.UserId
                           join role in _context.Roles on userrole.RoleId equals role.Id
                           where (role.Name == "Admin")
                           select (new DealerVM
                           {
                               UserName = user.UserName,
                               Role = role.Name,
                               Email = user.Email,
                               StatusType = user.StatusType.ToString()

                           }));



                //var dealers = await _context.Users
                //    .Join(_context.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { User = u, RoleId = ur.RoleId })
                //    .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { User = ur.User, Role = r })
                //    .Where(ur =>ur.Role.Name == "Dealer")
                //    .Select(ur => new DealerVM
                //    {
                //        UserName = ur.User.UserName,
                //        Role = ur.Role.Name,
                //        StatusType = ur.User.StatusType.ToString(),

                //    }).ToListAsync();
                return View(dealers);
           
        }

        public IActionResult RegisterDealers()
        {
            return View();
        }

        public IActionResult RegisterUsers()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginUser(Login model)
        {
            var user = await userManager.FindByEmailAsync(model.Username);
          //  var result = await signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);
            var result = await signInManager.PasswordSignInAsync(user.UserName, model.Password, false, true);
            var userRoles = await userManager.GetRolesAsync(user);
            var currentrole =  userRoles.FirstOrDefault();
            if (result.Succeeded)
            {
                HttpContext.Response.Cookies.Append("user", user.UserName);
                if (currentrole.Equals("SuperAdmin"))
                {
                    return RedirectToAction(currentrole.ToString());
                }
                else if (userRoles.Contains("Admin"))
                {
                    return RedirectToAction("Admin");
                }
                else if (userRoles.Contains("Dealer"))
                {
                    return RedirectToAction("Dealer");
                }

            }
           return RedirectToAction("Login");
        }








        [HttpPost]
        public async Task<IActionResult> Register( Register model)
        {
            var userExists = await userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return Ok("User already exists!");

            ApplicationUser user = new ApplicationUser()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };
            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return Ok("User creation failed! Please check user details and try again.");

            return Ok("User created successfully!");
        }




        [HttpPost]
        [Route("register-Dealer")]
        public async Task<IActionResult> RegisterDealer(Register model)
        {
            var userExists = await userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return Ok("User already exists!");

            ApplicationUser user = new ApplicationUser()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username,
                City = model.City,
                State = model.State,
                PhoneNumber = model.PhoneNumber,
                PinCode = model.PinCode,
                StatusType = SType.Pending,
                IsActive = false.ToString(),
            };
            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return Ok("User creation failed! Please check user details and try again.");
          
            if (await roleManager.RoleExistsAsync(UserRoles.Dealer))
            {
                await userManager.AddToRoleAsync(user, UserRoles.Dealer);
            }

            return Ok("User created successfully!");
        }


        [HttpPost]
        [Route("register-User")]
        public async Task<IActionResult> RegisterUser(Register model)
        {
            var userExists = await userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return Ok("User already exists!");

            ApplicationUser user = new ApplicationUser()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username,
                City = model.City,
                State = model.State,
                PhoneNumber = model.PhoneNumber,
                PinCode = model.PinCode,
            };
            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return Ok("User creation failed! Please check user details and try again.");

            if (await roleManager.RoleExistsAsync(UserRoles.User))
            {
                await userManager.AddToRoleAsync(user, UserRoles.User);
            }

            return Ok("User created successfully!");
        }




        [HttpPost]
        public async Task<IActionResult> AddAdmin(Register model)
        {
            var userExists = await userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return Ok("Admin already exists!");

            ApplicationUser user = new ApplicationUser()
            {

                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username,
                City = model.City,
                State = model.State,
                PhoneNumber = model.PhoneNumber,
                PinCode = model.PinCode,
               

            };
            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return Ok("User creation failed! Please check user details and try again.");
            if(!await roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await roleManager.CreateAsync(new IdentityRole  (UserRoles.Admin));
            }

            if (await roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await userManager.AddToRoleAsync(user, UserRoles.Admin);
            }

            return Ok("Admin created successfully!");
        }


        public async Task <IActionResult> Approve(string data)
        {
            var dealer = await userManager.FindByEmailAsync(data);
            dealer.StatusType = SType.Approved;
            _context.Update(dealer);
            await  _context.SaveChangesAsync();
            var message = new Message(new string[] { dealer.Email }, "Approval Status", "You are Approved Successfully...  ");
            _emailSender.SendEmail(message);
            return RedirectToAction("Admin");
        }
        public async Task <IActionResult> Reject(string email , string Reason)
        {
            var dealer = await userManager.FindByEmailAsync(email);

            dealer.StatusType = SType.Rejected;
            dealer.Reason = Reason;
            _context.Update(dealer);
            await _context.SaveChangesAsync();
            var message = new Message(new string[] { dealer.Email }, "Status for Dealer Registration", "You are Rejected For Following Reason...  " +Reason);
            _emailSender.SendEmail(message);
            return RedirectToAction("Admin");
        }

        public async Task<IActionResult> Block(string data)
        {
            var dealer = await userManager.FindByEmailAsync(data);

            dealer.StatusType = SType.Blocked;
            _context.Update(dealer);
            await _context.SaveChangesAsync();
            return RedirectToAction("Admin");
        }

        public async Task<IActionResult> UnBlock(string data)
        {
            var dealer = await userManager.FindByEmailAsync(data);

            dealer.StatusType = SType.Pending;
            _context.Update(dealer);
            await _context.SaveChangesAsync();
            return RedirectToAction("Admin");
        }


        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            HttpContext.Response.Cookies.Delete("user");

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Popup(string email)
        {
            ViewBag.Email = email;
            return PartialView("_Popup");
        }



        
    }
}
