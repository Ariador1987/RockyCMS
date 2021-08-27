using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Rocky.Data;
using Rocky.Models;
using Rocky.Models.ViewModels;
using Rocky.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Rocky.Controllers
{
    // cart-u smiju pristupiti samo authorizirani korisnici
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailSender _emailSender;
        // ovdje možemo koristit [BindProperty] da nemoramo koristit u post metodi
        [BindProperty]
        public ProductUserVM productUserVM { get; set; }
        public CartController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment, IEmailSender emailSender)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                // session exsists
                shoppingCartList = HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).ToList();
            }

            // distinct items
            List<int> prodInCart = shoppingCartList.Select(x => x.ProductId).ToList();
            // LINQ izraz se prijevodi na IN klauzulu u SQLu koristeći Contains ovdje.
            IEnumerable<Product> prodList = _db.Product.Where(x => prodInCart.Contains(x.Id));

            return View(prodList);
        }

        public IActionResult Remove(int id)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                // session exsists
                shoppingCartList = HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).ToList();
            }

            // item to remove
            shoppingCartList.Remove(shoppingCartList.FirstOrDefault(x => x.ProductId == id));
            // reset session state post-removal of item
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);

            return RedirectToAction(nameof(Index));
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken, ActionName("Index")]
        //public IActionResult IndexPost()
        //{
        //    return RedirectToAction(nameof(Summary));
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public IActionResult IndexPost()
        {
            return RedirectToAction(nameof(Summary));
        }

        public IActionResult Summary()
        {
            // Ovdje želimo prikazati iz podatke korisnika (ApplicationUsera), za to nam treba ID logiranog usera
            // za to ovdje koristimo Claims Identity.
            // NAČIN 1
            //var claimsIdentity = (ClaimsIdentity)User.Identity;
            //var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            // NAČIN 2
            var userId = User.FindFirst(ClaimTypes.NameIdentifier);

            // sad nam treba shoppingCart
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                // session exsists
                shoppingCartList = HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).ToList();
            }

            // distinct items
            List<int> prodInCart = shoppingCartList.Select(x => x.ProductId).ToList();
            // LINQ izraz se prijevodi na IN klauzulu u SQLu koristeći Contains ovdje.
            IEnumerable<Product> prodList = _db.Product.Where(x => prodInCart.Contains(x.Id));

            productUserVM = new ProductUserVM()
            {
                ApplicationUser = _db.ApplicationUser.FirstOrDefault(x => x.Id == userId.Value),
                ProductList = prodList.ToList()
            };

            return View(productUserVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //[ActionName("Summary")]
        public async Task<IActionResult> SummaryPost(ProductUserVM productUserVM)
        {
            var pathToTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
                                + "templates" + Path.DirectorySeparatorChar.ToString() +
                                "Inquiry.html";

            var subject = "New Inquiry";
            var htmlBody = "";
            using (StreamReader sr = new StreamReader(pathToTemplate))
            {
                htmlBody = sr.ReadToEnd();
            }

            StringBuilder productListSB = new StringBuilder();
            foreach (var prod in productUserVM.ProductList)
            {
                productListSB.Append($" - Name: {prod.Name} <span style='font-size:14px;'> (ID: {prod.Id})<span><br />");
            }
            string messageBody = string.Format(htmlBody,
                    productUserVM.ApplicationUser.FullName,
                    productUserVM.ApplicationUser.Email,
                    productUserVM.ApplicationUser.PhoneNumber,
                    productListSB.ToString()
                    );

            await _emailSender.SendEmailAsync(WC.EmailAdmin, subject, messageBody);

            return RedirectToAction(nameof(InquiryConfirmation));
        }

        public IActionResult InquiryConfirmation()
        {
            HttpContext.Session.Clear();
            return View();
        }
    }
}
