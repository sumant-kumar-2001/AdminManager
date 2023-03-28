using AdminManager.Authentication;
using AdminManager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, SignInManager<ApplicationUser> signInManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.roleManager = roleManager;
            _configuration = configuration;
        }


        public IActionResult GetAllDealer()
        {
            var a = userManager.GetUsersInRoleAsync(UserRoles.Dealer);
            return View();
        }




        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Dealer()
        {
            return View();
        }

        public IActionResult SuperAdmin()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> LoginUser(Login model)
        {
            var user = await userManager.FindByNameAsync(model.Username);
            var result = await signInManager.PasswordSignInAsync( model.Username, model.Password, false, false);
            var userRole = await userManager.GetUsersInRoleAsync(UserRoles.SuperAdmin);

            
            if (result.Succeeded)

            {
                if (userRole.FirstOrDefault() == UserRoles.SuperAdmin)
                {

                    return RedirectToAction("GetAllDealer");
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
                UserName = model.Username
            };
            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return Ok("User creation failed! Please check user details and try again.");

            if (!await roleManager.RoleExistsAsync(UserRoles.Dealer))
                await roleManager.CreateAsync(new IdentityRole(UserRoles.Dealer));
            if (!await roleManager.RoleExistsAsync(UserRoles.User))
                await roleManager.CreateAsync(new IdentityRole(UserRoles.User));

            if (await roleManager.RoleExistsAsync(UserRoles.Dealer))
            {
                await userManager.AddToRoleAsync(user, UserRoles.Dealer);
            }

            return Ok("User created successfully!");
        }

    }
}
