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
    public class InventoryTransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InventoryTransactionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: InventoryTransactions
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.InventoryTransactions.Include(i => i.Product);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: InventoryTransactions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventoryTransaction = await _context.InventoryTransactions
                .Include(i => i.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inventoryTransaction == null)
            {
                return NotFound();
            }

            return View(inventoryTransaction);
        }

        // GET: InventoryTransactions/Create
        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name");
            return View();
        }

        // POST: InventoryTransactions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProductId,Quantity,TransactionDate")] InventoryTransaction transaction)
        {
            if (ModelState.IsValid)
            {
                var product = await _context.Products.FindAsync(transaction.ProductId);
                if (product != null)
                {
                    product.Stock += transaction.Quantity; // Uppdatera lagret
                }

                _context.InventoryTransactions.Add(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", transaction.ProductId);
            return View(transaction);
        }


        // GET: InventoryTransactions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventoryTransaction = await _context.InventoryTransactions.FindAsync(id);
            if (inventoryTransaction == null)
            {
                return NotFound();
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", inventoryTransaction.ProductId);
            return View(inventoryTransaction);
        }

        // POST: InventoryTransactions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProductId,Quantity,TransactionDate")] InventoryTransaction transaction)
        {
            if (id != transaction.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingTransaction = await _context.InventoryTransactions
                        .AsNoTracking()
                        .FirstOrDefaultAsync(t => t.Id == id);

                    if (existingTransaction == null)
                    {
                        return NotFound();
                    }

                    var product = await _context.Products.FindAsync(transaction.ProductId);
                    if (product != null)
                    {
                        // Justera lagret baserat på skillnaden i Quantity
                        product.Stock += transaction.Quantity - existingTransaction.Quantity;
                    }

                    _context.Update(transaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InventoryTransactionExists(transaction.Id))
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

            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", transaction.ProductId);
            return View(transaction);
        }


        // GET: InventoryTransactions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventoryTransaction = await _context.InventoryTransactions
                .Include(i => i.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inventoryTransaction == null)
            {
                return NotFound();
            }

            return View(inventoryTransaction);
        }

        // POST: InventoryTransactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inventoryTransaction = await _context.InventoryTransactions.FindAsync(id);
            if (inventoryTransaction != null)
            {
                _context.InventoryTransactions.Remove(inventoryTransaction);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InventoryTransactionExists(int id)
        {
            return _context.InventoryTransactions.Any(e => e.Id == id);
        }
    }
}
