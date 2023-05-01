using AdminManager.Data;
using AdminManager.Models;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System;
using Microsoft.AspNetCore.Identity;
using AdminManager.Authentication;
using Microsoft.AspNetCore.Authorization;
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
            IEmailSender emailSender, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment webHostEnvironment)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.roleManager = roleManager;
            _configuration = configuration;
            _context = context;
            _emailSender = emailSender;
            _httpContextAccessor = httpContextAccessor;
            _webHostEnvironment = webHostEnvironment;
        }


        [Authorize(Roles = "Dealer")]
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

        [Authorize(Roles = "SuperAdmin,Admin")]
        public IActionResult UpdateIsActive(int? id)
        {
            Product product = _context.Products.Find(id);
            if (product.IsActive == true)
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

        [Authorize(Roles = "Dealer")]
        [HttpGet]
        public IActionResult AddDiscount()
        {
            return View();
        }

        [Authorize(Roles = "Dealer")]
        [HttpPost]
        public IActionResult AddDiscount(Discount discount, int? id)
        {
            var currentrole = HttpContext.Session.GetString("UserRole");
            var product = _context.Products.Find(id);

            if (ModelState.IsValid)
            {

                if (discount.ValidFrom > discount.ValidTo)
                {
                    ModelState.AddModelError("discount.ValidTo", "ToDate is Must be greater than FromDate");
                    return View(discount);

                }

                if (discount.DiscountType == "Fixed Amount")
                {
                    if (discount.DiscountAmount > product.Price)
                    {
                        ModelState.AddModelError("discount.DiscountAmount", "DiscountAmount can't be greater than price of product");
                        return View(discount);
                    }
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
                    if (discount.DiscountAmount > 100)
                    {
                        ModelState.AddModelError("discount.DiscountAmount", "Amount can't be dicounted more than 100%");
                        return View(discount);
                    }

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
                if (ExistDiscount == null)
                {
                    _context.discount.Add(discount);
                    _context.SaveChanges();
                }
                else
                {
                    _context.discount.Update(discount);
                    _context.SaveChanges();
                }

                return RedirectToAction("Allproduct", "User", "User");


            }
            return View(discount);
        }


        [Authorize(Roles = "Dealer")]
        public IActionResult DeleteProduct(int Id)
        {
            Product product = _context.Products.Find(Id);

            if (product == null)
            {
                return Json(new { success = false, message = "Error While Deleting" });
            }

            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            //foreach (Discount item in _context.discount.Where(x => x.ProductId == Id))
            //{
            //    _context.discount.Remove(item);
            //    _context.SaveChanges();


            //}


            _context.discount.Where(p => p.ProductId == Id)
              .ToList().ForEach(p => _context.discount.Remove(p));
            _context.SaveChanges();
            _context.Products.Remove(product);
            _context.SaveChanges();

            return RedirectToAction("Allproduct", "User", "User");

        }



        [Authorize(Roles = "Dealer")]
        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product, IFormFile? file)
        {


            if (ModelState.IsValid)
            {
                ViewBag.Dealer = HttpContext.Session.GetString("Dealer");

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

                if (product.ProductId == 0)
                {
                    product.UserId = ViewBag.Dealer;
                    _context.Products.Add(product);
                    TempData["success"] = "Product Added Successfully";

                }
                else
                {
                    _context.Products.Update(product);
                    TempData["success"] = "Product Updated Successfully";
                }
                _context.SaveChanges();
                return RedirectToAction("Allproduct", "User", "User");
            }

            return View(product);

            //product = new Product()
            //{
            //    UserId = ViewBag.Id,
            //    Price = product.Price,
            //    Quantity = product.Quantity,
            //    ImageUrl = product.ImageUrl,
            //    Description = product.Description,
            //    Name = product.Name

            //};
        }

    }
}
