using CSC595_Week04.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CSC595_Week04.Controllers
{
    /// <summary>
    /// Handles all CRUD operations for the game library.
    /// Add, Edit, and Delete actions require an authenticated user.
    /// </summary>
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ProductDBContext _dbContext;

        private const int PageSize = 8;

        public ProductController(ProductDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET /Product/Index — List games with optional name search, genre filter, sort, and pagination
        public IActionResult Index(string? searchByName, string? genre, string? sortBy, int page = 1)
        {
            var products = _dbContext.Products.AsEnumerable();

            // Filter by name (case-insensitive)
            if (!string.IsNullOrEmpty(searchByName))
                products = products.Where(p => p.Name!.ToLower().Contains(searchByName.ToLower()));

            // Filter by genre
            if (!string.IsNullOrEmpty(genre))
                products = products.Where(p => p.Genre == genre);

            // Sort
            products = sortBy switch
            {
                "name_desc"  => products.OrderByDescending(p => p.Name),
                "price_asc"  => products.OrderBy(p => p.Price),
                "price_desc" => products.OrderByDescending(p => p.Price),
                _            => products.OrderBy(p => p.Name)  // default: name_asc
            };

            // Pagination
            int totalCount = products.Count();
            int totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            var paged = products.Skip((page - 1) * PageSize).Take(PageSize).ToList();

            // Build base64 image dictionary for the paged products
            Dictionary<int, string> photos = new();
            foreach (var product in paged)
            {
                if (product.ImageDataForProduct != null)
                {
                    string b64 = Convert.ToBase64String(product.ImageDataForProduct);
                    photos[product.Id] = $"data:image/jpg;base64,{b64}";
                }
            }

            ViewBag.Photos       = photos;
            ViewBag.SearchByName = searchByName;
            ViewBag.Genre        = genre;
            ViewBag.SortBy       = sortBy;
            ViewBag.Page         = page;
            ViewBag.TotalPages   = totalPages;
            ViewBag.TotalCount   = totalCount;
            ViewBag.Genres       = Product.Genres;

            return View(paged);
        }

        public IActionResult ShowAll() => RedirectToAction("Index");
        public IActionResult DisplayAll() => View("Index", _dbContext.Products.ToList());

        // GET /Product/ShowDetails/{id}
        public IActionResult ShowDetails(int id)
        {
            Product? product = _dbContext.Products.FirstOrDefault(p => p.Id == id);

            if (product?.ImageDataForProduct != null)
            {
                string b64 = Convert.ToBase64String(product.ImageDataForProduct);
                ViewBag.productProfilePhoto = $"data:image/jpg;base64,{b64}";
            }

            ViewBag.id = id;
            return View(product);
        }

        // GET /Product/Add
        [Authorize]
        [HttpGet]
        public IActionResult Add()
        {
            ViewBag.Genres = Product.Genres;
            return View();
        }

        // POST /Product/Add
        [HttpPost]
        public IActionResult Add(Product newProduct)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Genres = Product.Genres;
                return View();
            }

            // Read uploaded cover image into a byte array for database storage
            foreach (var file in Request.Form.Files)
            {
                using MemoryStream ms = new();
                file.CopyTo(ms);
                newProduct.ImageDataForProduct = ms.ToArray();
            }

            _dbContext.Products.Add(newProduct);
            _dbContext.SaveChanges();

            TempData["Success"] = $"\"{newProduct.Name}\" was added to the library.";
            return RedirectToAction("Index");
        }

        // GET /Product/Edit/{id}
        [Authorize]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            Product? product = _dbContext.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();

            ViewBag.Genres = Product.Genres;
            return View(product);
        }

        // POST /Product/Edit
        [HttpPost]
        public IActionResult Edit(Product changedProduct)
        {
            Product? existing = _dbContext.Products.FirstOrDefault(p => p.Id == changedProduct.Id);

            if (!ModelState.IsValid)
            {
                ViewBag.Genres = Product.Genres;
                return View(existing);
            }

            if (existing == null) return NotFound();

            existing.Name  = changedProduct.Name;
            existing.Price = changedProduct.Price;
            existing.Genre = changedProduct.Genre;
            _dbContext.SaveChanges();

            TempData["Success"] = $"\"{existing.Name}\" was updated successfully.";
            return RedirectToAction("Index");
        }

        // GET /Product/Delete/{id} — Show confirmation page
        [Authorize]
        [HttpGet]
        public IActionResult Delete(int id)
        {
            Product? product = _dbContext.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST /Product/Delete — Perform deletion
        [HttpPost]
        [ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            Product? product = _dbContext.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();

            string name = product.Name;
            _dbContext.Products.Remove(product);
            _dbContext.SaveChanges();

            TempData["Success"] = $"\"{name}\" was removed from the library.";
            return RedirectToAction("Index");
        }
    }
}
