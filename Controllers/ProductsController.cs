using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StockSystemApp.Data;
using StockSystemApp.Models;

namespace StockSystemApp.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index(string sortOrder, string searchString)
        {
            // Store the sorting parameters in ViewData for use in the view
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DescriptionSortParm"] = sortOrder == "Description" ? "description_desc" : "Description";
            ViewData["StockSortParm"] = sortOrder == "Stock" ? "stock_desc" : "Stock";
            ViewData["CategorySortParm"] = sortOrder == "Category" ? "category_desc" : "Category";
            ViewData["CurrentFilter"] = searchString; // Store the current search term in ViewData

            // Get the products from the database and include the Category
            var products = from p in _context.Products.Include(p => p.Category)
                           select p;

            // Apply the search filter if a search term is provided
            if (!String.IsNullOrEmpty(searchString))
            {
                string lowerSearchString = searchString.ToLower(); // Convert search string to lowercase

                // Use ToLower() on the fields as well for case-insensitive comparison
                products = products.Where(p => p.Name.ToLower().Contains(lowerSearchString) ||
                                               p.Description.ToLower().Contains(lowerSearchString) ||
                                               p.Category.Name.ToLower().Contains(lowerSearchString));
            }

            // Apply the sorting based on the sortOrder parameter
            switch (sortOrder)
            {
                case "name_desc":
                    products = products.OrderByDescending(p => p.Name);
                    break;
                case "Description":
                    products = products.OrderBy(p => p.Description);
                    break;
                case "description_desc":
                    products = products.OrderByDescending(p => p.Description);
                    break;
                case "Stock":
                    products = products.OrderBy(p => p.Stock);
                    break;
                case "stock_desc":
                    products = products.OrderByDescending(p => p.Stock);
                    break;
                case "Category":
                    products = products.OrderBy(p => p.Category.Name);
                    break;
                case "category_desc":
                    products = products.OrderByDescending(p => p.Category.Name);
                    break;
                default:
                    products = products.OrderBy(p => p.Name);
                    break;
            }

            // Return the filtered and sorted list of products to the view
            return View(await products.AsNoTracking().ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Stock,CategoryId")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Stock,CategoryId")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = await _context.Products
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.Id == id);

                    if (existingProduct == null)
                    {
                        return NotFound();
                    }

                    // Kontrollera om Stock har ändrats
                    if (existingProduct.Stock != product.Stock)
                    {
                        var transaction = new InventoryTransaction
                        {
                            ProductId = product.Id,
                            Quantity = product.Stock - existingProduct.Stock, // Beräknar ändringen i lager
                            Timestamp = DateTime.Now
                        };

                        _context.InventoryTransactions.Add(transaction);
                    }

                    // Uppdatera produkten
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }


        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
