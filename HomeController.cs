using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModel;
using Spice.Utility;

namespace Spice.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<IActionResult> Index()
        {

            IndexViewModel IndexVM = new IndexViewModel()
            {
                menuItem = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).ToListAsync(),
                category = await _db.Category.ToListAsync(),
                coupon = await _db.Coupon.Where(c=>c.IsActive ==true).ToListAsync()
            };



            //var claimsIdentity = (ClaimsIdentity)User.Identity;
            //var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            //if(claims != null)
            //{
            //    var count = _db.ShoppingCard.Where(u => u.ApplicationUserId == claims.Value).Count();
            //    HttpContext.Session.SetInt32("ssCartCount", count);

            //}
            return View(IndexVM);
        }


        [HttpGet]
        public async Task<IActionResult> Details(int Id)
        {
            var menuItemDb = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).
                Where(m => m.Id == Id).FirstOrDefaultAsync();
            ShoppingCard shoppingCard = new ShoppingCard()
            {
                MenuItem = menuItemDb,
                MenuItemId = menuItemDb.Id
            };
            return View(shoppingCard);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(ShoppingCard cardObject)
        {
            cardObject.Id = 0;
            if (ModelState.IsValid)
            {
                var claimIdentity = (ClaimsIdentity)this.User.Identity;
                var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
                cardObject.ApplicationUserId = claim.Value;
                ShoppingCard cardFromDb = await _db.ShoppingCard.Where(c => c.ApplicationUserId == cardObject.ApplicationUserId
                && c.MenuItemId == cardObject.MenuItemId).FirstOrDefaultAsync();
                if(cardFromDb == null)
                {
                    await _db.ShoppingCard.AddAsync(cardObject);
                }
                else
                {
                    cardFromDb.Count = cardFromDb.Count + cardObject.Count;
                }
                await _db.SaveChangesAsync();
                var count = _db.ShoppingCard.Where(c => c.ApplicationUserId == cardObject.ApplicationUserId).Count();
                HttpContext.Session.SetInt32(SD.ssShoppingCartCount, count);
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var menuItemDb = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).
               Where(m => m.Id == cardObject.MenuItemId).FirstOrDefaultAsync();
                ShoppingCard shoppingCard = new ShoppingCard()
                {
                    MenuItem = menuItemDb,
                    MenuItemId = menuItemDb.Id
                };
                return View(menuItemDb);
            }

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
}
