using AdminManager.Authentication;
using AdminManager.Data;
using AdminManager.Models;
using AdminManager.Models.Email;
using EcommerceFrontend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EcommerceFrontend.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context,UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            this.roleManager = roleManager;
            this.signInManager = signInManager;
        }

        public IActionResult Index( string? term)
        {
            if (term == null)
            {
                IEnumerable<Product> products = _context.Products.Where(x => x.IsActive == true).ToList();
                return View(products);
            }
            else
            {
                IEnumerable<Product> products = _context.Products.Where(x => x.Name.ToLower().Contains(term) && x.IsActive == true).ToList();
                return View(products);
            }         
        }

        public IActionResult RegisterUser()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> Login(Login model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            var result = await signInManager.PasswordSignInAsync(user.UserName, model.Password, false, false);
            var userRoles = await _userManager.GetRolesAsync(user);
            var currentrole = userRoles.FirstOrDefault();

            if (result.Succeeded)
            {
                //HttpContext.Session.SetString("user", user.UserName);
                //HttpContext.Session.SetString("UserId", user.Id);
                //HttpContext.Session.SetString("UserRole", currentrole);

                if (currentrole.Equals("User"))
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    HttpContext.Session.Clear();
                    return View("Page");
                }
            }
            return RedirectToAction("Login");
        }






        [HttpPost]  
        public async Task<IActionResult> RegisterUser(Register model)
         {
            if (ModelState.IsValid)
            {
                var userExists = await _userManager.FindByEmailAsync(model.Email);
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
                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                    return Ok("User creation failed! Please check user details and try again.");

                if (await roleManager.RoleExistsAsync(UserRoles.User))
                {
                    await _userManager.AddToRoleAsync(user, UserRoles.User);
                }

                return View("Login");
            }
            else
            {
                return View("RegisterUsers");
            }
        }

        //public IActionResult SearchTerm(string? term)
        //{
        //    if(term == null)
        //    {
        //        ViewBag.product = _context.Products.ToList();
        //    }
        //    else
        //    {
        //        IEnumerable<Product> products = _context.Products.Where(x => x.Name == term).ToList();
        //        return View("Index");
        //    }
        // return View ("Index");
        //}

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new AdminManager.Models.ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }





        public IActionResult Details(int id)
        {
           var product =  _context.Products.Where(x => x.ProductId == id).First();

            return View(product);
        }
    }
}