using CSC595_Week04.Models;
using Microsoft.AspNetCore.Mvc;

namespace CSC595_Week04.Controllers
{
    /// <summary>
    /// Serves the public-facing home/landing page with library stats.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ProductDBContext _dbContext;

        public HomeController(ProductDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET / — Landing page showing library stats and recently added games
        public IActionResult Index()
        {
            ViewBag.TotalGames  = _dbContext.Products.Count();
            ViewBag.TotalGenres = _dbContext.Products.Select(p => p.Genre).Distinct().Count();
            ViewBag.RecentGames = _dbContext.Products
                .OrderByDescending(p => p.Id)
                .Take(3)
                .ToList();

            return View();
        }

        public ActionResult Error()
        {
            return View();
        }
    }
}
