using AdminManager.Data;
using AdminManager.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AdminManager.Areas.Customer.Controllers;

[Area("Customer")]

        public class HomeController : Controller
        {
            private readonly ILogger<HomeController> _logger;
            private readonly ApplicationDbContext _context;

            public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
            {
                _logger = logger;
               _context = context;
            }

            public IActionResult Index()
            {
                 IEnumerable<Product> productlist = _context.Products.ToList();
                 return View(productlist);
             }



            public IActionResult Privacy()
            {
                return View();
            }

            [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
            public IActionResult Error()
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }





        }
    

