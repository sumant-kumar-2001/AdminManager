using AdminManager.Authentication;
using AdminManager.Data;
using AdminManager.Models;
using AdminManager.Models.Email;
using AdminManager.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Printing;
//using Microsoft.IdentityModel.Tokens;
using System.Dynamic;
//using System.IdentityModel.Tokens.Jwt;
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

        [Authorize(Roles = "SuperAdmin,Admin")]

        public async Task<IActionResult> Admin()
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

        [Authorize(Roles = "SuperAdmin")]
        public IActionResult AddRole()
        {
            var roles = roleManager.Roles.ToList();
            return View(roles);
        }
        [Authorize(Roles = "SuperAdmin")]

        public IActionResult AddAdmin()
        {
            return View();
        }
        public IActionResult AddRoles()
        {
            return View(new IdentityRole());
        }

        [Authorize(Roles = "SuperAdmin")]

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
        [Authorize(Roles = "Dealer")]
        public IActionResult Dealer(int? Page)
        {
            int pageSize = 2;
            var user = HttpContext.Session.GetString("Dealer");

            //IEnumerable<Product> products;
            //products = (from product in _context.Products
            //            where product.UserId == HttpContext.Session.GetString("UserId")
            //            select (new Product
            //            {
            //                ProductId = product.ProductId,
            //                Name = product.Name,
            //                Price = product.Price,
            //                IsActive = product.IsActive,
            //                Quantity = product.Quantity,
            //                DiscountAmount = product.DiscountAmount,
            //            }));

            return View(PageList<Product>.Create(_context.Products.Where(x=> x.UserId == user).ToList(), Page ?? 1, pageSize));
        }


        [Authorize(Roles = "SuperAdmin,Admin,Dealer")]
        [HttpGet]
        [HttpPost]
        public IActionResult Allproduct(int? Page, string? data , string? term)
        
        
        {
            int pageSize = 2;
            if(term == null)
            {
                term = "";
            }

            if (User.IsInRole("SuperAdmin")|| User.IsInRole("Admin"))
            {

                if (data != null)
                {
                    ViewBag.SearchStr = term;
                    ViewBag.Dealer = data;
                  return View(PageList<Product>.Create(_context.Products.Where( x =>  x.UserId == data && x.Name.ToLower().Contains( term.ToLower())).ToList(), Page ??     1, pageSize));
                }
                else
                {
                    ViewBag.SearchStr = term;
                    return View(PageList<Product>.Create(_context.Products.Where(x => x.Name.ToLower().Contains(term)).ToList(), Page ?? 1, pageSize));
                }
            }
            if (User.IsInRole("Dealer"))
            {
                var user = HttpContext.Session.GetString("Dealer");
                ViewBag.Dealer = data;
                ViewBag.SearchStr = term;

                return View(PageList<Product>.Create(_context.Products.Where(x => x.UserId == user && x.Name.ToLower().Contains(term)).ToList(), Page ?? 1, pageSize));

            }
            else
            {
                return NotFound();
            }

        }



        [Authorize(Roles = "SuperAdmin")]

        [HttpGet]
        public IActionResult SuperAdmin()
        {
            //var User = HttpContext.Session.GetString("user");
           //var Customer = User.Identity.Name;

            if (!User.Identity.IsAuthenticated)
            {
                return View("Page");
            }

            IEnumerable<DealerVM> admin;
            admin = (from user in _context.ApplicationUsers
                     join userrole in _context.UserRoles on user.Id equals userrole.UserId
                     join role in _context.Roles on userrole.RoleId equals role.Id
                     where (role.Name == "Admin")
                     select (new DealerVM
                     {
                         UserName = user.UserName,
                         Email = user.Email,
                         PhoneNumber = user.PhoneNumber,

                     }));

            return View(admin);
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
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {

                //  var result = await signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);
                var result = await signInManager.PasswordSignInAsync(user.UserName, model.Password, false, false);
                var userRoles = await userManager.GetRolesAsync(user);
                var currentrole = userRoles.FirstOrDefault();

                if (result.Succeeded)
                {
                    // HttpContext.Response.Cookies.Append("user", user.UserName);
                    HttpContext.Session.SetString("user", user.UserName);
                    HttpContext.Session.SetString("Dealer", user.Email);
                    HttpContext.Session.SetString("UserRole", currentrole);

                    if (currentrole.Equals("SuperAdmin") || currentrole.Equals("Admin") || currentrole.Equals("User") || currentrole.Equals("Dealer"))
                    {
                        if (currentrole.Equals("Dealer"))
                        {
                            if (user.StatusType == SType.Approved)
                            {
                                return RedirectToAction("Allproduct");
                            }
                            else
                            {
                                HttpContext.Session.Clear();
                                return View("Page");
                            }
                        }
                        else
                        {
                            return RedirectToAction(currentrole.ToString());
                        }
                    }
                }
                      TempData["error"] = "Invalid Password";
                return RedirectToAction("Login");
            }
            TempData["error"] = "User Doesn't Exist";
            return RedirectToAction("Login");
        }




        [HttpPost]
        [Route("register-Dealer")]
        public async Task<IActionResult> RegisterDealer(Register model)
        {
            if (ModelState.IsValid)
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
                    PinCode = model.PinCode,
                    PhoneNumber = model.PhoneNumber,
                    StatusType = SType.Pending

                };


                var result = await userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                    return Ok("User creation failed! Please check user details and try again.");

                //if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
                //    await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
                //if (!await roleManager.RoleExistsAsync(UserRoles.User))
                //    await roleManager.CreateAsync(new IdentityRole(UserRoles.User));
                //if (!await roleManager.RoleExistsAsync(UserRoles.SuperAdmin))
                //    await roleManager.CreateAsync(new IdentityRole(UserRoles.SuperAdmin));
                //if (!await roleManager.RoleExistsAsync(UserRoles.Dealer))
                //    await roleManager.CreateAsync(new IdentityRole(UserRoles.Dealer));

                if (await roleManager.RoleExistsAsync(UserRoles.Dealer))
                {
                    await userManager.AddToRoleAsync(user, UserRoles.Dealer);
                }
                TempData["success"] = "Dealer Registered Successfully";
                return View("Login");

            }
            return View("RegisterDealers");


        }



        [Route("register-User")]
        public async Task<IActionResult> RegisterUser(Register model)
        {
            if (ModelState.IsValid)
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
                TempData["success"] = "User Registered Successfully";
                return View("Login");
            }
            else
            {
                return View("RegisterUsers");
            }
        }



        [Authorize(Roles = "SuperAdmin")]
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
            TempData["success"] = "Admin Registered Successfully";
            return RedirectToAction("SuperAdmin");
        }

        [Authorize(Roles = "SuperAdmin,Admin")]

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
        [Authorize(Roles = "SuperAdmin,Admin")]
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
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Block(string data)
        {
            var dealer = await userManager.FindByEmailAsync(data);

            dealer.StatusType = SType.Blocked;
            _context.Update(dealer);
            await _context.SaveChangesAsync();
            return RedirectToAction("Admin");
        }
        [Authorize(Roles = "SuperAdmin,Admin")]
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
            //HttpContext.Response.Cookies.Delete("user");
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet]
        public IActionResult Popup(string email)
        {
            ViewBag.Email = email;
            return PartialView("_Popup");
        }

        public IActionResult Back()
        {
            var currentrole = HttpContext.Session.GetString("UserRole");

            if (currentrole == "SuperAdmin")
            {
                return RedirectToAction("SuperAdmin", "User", "User");
            }
            else if(currentrole == "Admin")
            {
                return RedirectToAction("Admin", "User", "User");
            }
            else if(currentrole == "Dealer")
            {
                return RedirectToAction("Allproduct");
            }
            else
            {
                return NotFound();
            }
        }


    }
}
