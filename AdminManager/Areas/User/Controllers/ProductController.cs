using AdminManager.Data;
using AdminManager.Models;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System;
using Microsoft.AspNetCore.Identity;
using AdminManager.Authentication;
using AdminManager.Models.Email;
using Org.BouncyCastle.Bcpg;
using System.Linq;
using Microsoft.Extensions.Hosting;
using AdminManager.Models;
using System.Net;

namespace AdminManager.Areas.User.Controllers
{
    [Area("User")]
    public class ProductController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
            IConfiguration configuration, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context,
            IEmailSender emailSender, IHttpContextAccessor httpContextAccessor,IWebHostEnvironment webHostEnvironment )
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.roleManager = roleManager;
            _configuration = configuration;
            _context = context;
            _emailSender = emailSender;
            _httpContextAccessor = httpContextAccessor;
            _webHostEnvironment= webHostEnvironment;
        }
        [HttpGet]
        public IActionResult AddProduct(int? id)
        {
            Product product = new Product();
            if (id == null || id == 0)
            {          
                return View(product);
            }
            else
            {
                product = _context.Products.Find(id);
                return View(product);
            }

        }


        public IActionResult UpdateIsActive(int? id)
        {
            Product product = _context.Products.Find(id);
            if(product.IsActive == true)
            {
                product.IsActive = false;
            }
            else
            {
                product.IsActive = true;
            }
            _context.Products.Update(product);
            _context.SaveChanges();
            return RedirectToAction("Allproduct", "User", "User");
        }




        [HttpGet]
        public IActionResult AddDiscount()
        {
            return View();
        }


        [HttpPost]
        public IActionResult AddDiscount(Discount discount , int? id)
        {
         var   currentrole = HttpContext.Session.GetString("UserRole");
            var product = _context.Products.Find(id);
            if (discount.DiscountType == "Fixed Amount")
            {
                discount = new Discount
                {
                    DiscountType = discount.DiscountType,
                    ProductId = product.ProductId,
                    ValidFrom = discount.ValidFrom,
                    ValidTo = discount.ValidTo,
                    DiscountAmount = discount.DiscountAmount,
                };
               
            }
            else
            {
                discount = new Discount
                {
                    DiscountType = discount.DiscountType,
                    ProductId = product.ProductId,
                    ValidFrom = discount.ValidFrom,
                    ValidTo = discount.ValidTo,
                    DiscountAmount = (product.Price * discount.DiscountAmount) / 100
                };
            }
            product.DiscountAmount = product.Price - discount.DiscountAmount;

            var ExistDiscount = _context.discount.Where(x => x.ProductId == product.ProductId).FirstOrDefault();
            if (ExistDiscount == null )
            {
                _context.discount.Add(discount);
                _context.SaveChanges();
            }
            else
            {
                _context.discount.Update(discount);
                _context.SaveChanges();
            }


           

            if (currentrole == "SuperAdmin" || currentrole == "Admin")
            {
                return RedirectToAction("AllProduct", "User", "User");
            }
            else
            {
                return RedirectToAction("Dealer", "User", "User");

            }
        }



        [HttpDelete]
        public IActionResult DeleteProduct(int ? id)
        {
            Product product = _context.Products.Find(id);

            if(product == null)
            {
                return Json(new { success = false, message = "Error While Deleting" });
            }
            
                var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageUrl.TrimStart('\\'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            _context.Products.Remove(product);
            _context.SaveChanges();
            return RedirectToAction("Dealer", "User", "User");

        }




        [HttpPost]
        public async Task <IActionResult> AddProduct( Product product ,IFormFile? file)
        {


            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string ProductPath = Path.Combine(wwwRootPath, @"Images\Products");



                    if (product.ImageUrl != null)
                    {

                        var oldImagePath = Path.Combine(wwwRootPath, product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }

                    }
                    using (var fileStream = new FileStream(Path.Combine(ProductPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    product.ImageUrl = @"\Images\Products\" + fileName;
                }
            }
            return View(product);
            //product = new Product()
            //{
            //    UserId = ViewBag.Id,
            //    Price = product.Price,
            //    Quantity= product.Quantity,
            //    ImageUrl= product.ImageUrl,
            //    Description = product.Description,  
            //    Name = product.Name

            //};

            ViewBag.Id = HttpContext.Session.GetString("UserId");

            if (product.ProductId == 0)
            {
                 product.UserId = ViewBag.Id;
                _context.Products.Add(product);
                _context.SaveChanges();
                return RedirectToAction("Dealer", "User", "User");

            }
            else
            {
                _context.Products.Update(product);
                _context.SaveChanges();
                return RedirectToAction("Dealer", "User", "User");

            }         
            
        }

    }
}
