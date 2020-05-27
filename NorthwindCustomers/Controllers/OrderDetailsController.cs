using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NorthwindCustomers.Models;
using ReflectionIT.Mvc.Paging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NorthwindCustomers.Controllers
{
    public class OrderDetailsController : Controller
    {
        private readonly NorthwindContext _context;
        private object e;

        public OrderDetailsController(NorthwindContext context)
        {
            _context = context;
        }

        // GET: OrderDetails
        public async Task<IActionResult> Index(int page = 1)
        {
            var northwindContext = _context.OrderDetails.Include(o => o.Order).Include(o => o.Product).AsNoTracking().OrderBy(o => o.OrderId); ;
            var model = await PagingList.CreateAsync(northwindContext, 10, page);
            return View(model);

        }

        // GET: OrderDetails/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderDetails = await _context.OrderDetails
                .Include(o => o.Order)
                .Include(o => o.Product)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (orderDetails == null)
            {
                return NotFound();
            }

            return View(orderDetails);
        }

        // GET: OrderDetails/Create
        public IActionResult Create(int? id)
        {
            if (id != null)
            {
                var order = from c in _context.Orders where c.OrderId == id.Value select c;
                ViewData["OrderId"] = new SelectList(order, "OrderId", "OrderId");
            }
            else
            {
                ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "OrderId");
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductName");
            return View();
        }

        // POST: OrderDetails/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,ProductId,UnitPrice,Quantity,Discount")] OrderDetails orderDetails, string submitButton)
        {
            if (ModelState.IsValid)
            {
                switch (submitButton)
                {
                    case "Create Order":
                        {
                            _context.Add(orderDetails);
                            await _context.SaveChangesAsync();
                            return RedirectToAction(nameof(Index));
                        };
                    case "Add more products to order":
                        {
                            _context.Add(orderDetails);
                            await _context.SaveChangesAsync();
                            return RedirectToAction(nameof(Create), new { id = orderDetails.OrderId});
                        };
                }

            }
            ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "OrderId", orderDetails.OrderId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductName", orderDetails.ProductId);
            return View(orderDetails);
        }

        // GET: OrderDetails/Edit/5
        public async Task<IActionResult> Edit(int? id, int? productId)
        {
            if (id == null || productId == null)
            {
                return NotFound();
            }

            var orderDetails = await _context.OrderDetails.FindAsync(id, productId);
            if (orderDetails == null)
            {
                return NotFound();
            }
            ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "OrderId", orderDetails.OrderId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductName", orderDetails.ProductId);
            return View(orderDetails);
        }

        // POST: OrderDetails/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int productId, [Bind("OrderId,ProductId,UnitPrice,Quantity,Discount")] OrderDetails orderDetails)
        {
            if (id != orderDetails.OrderId || productId != orderDetails.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orderDetails);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderDetailsExists(orderDetails.OrderId, orderDetails.ProductId))
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
            ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "OrderId", orderDetails.OrderId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductName", orderDetails.ProductId);
            return View(orderDetails);
        }

        // GET: OrderDetails/Delete/5
        public async Task<IActionResult> Delete(int? id, int? productId)
        {
            if (id == null || productId == null)
            {
                return NotFound();
            }

            var orderDetails = await _context.OrderDetails
                .Include(o => o.Order)
                .Include(o => o.Product)
                .FirstOrDefaultAsync(m => m.OrderId == id && m.ProductId == productId);
            if (orderDetails == null)
            {
                return NotFound();
            }

            return View(orderDetails);
        }

        // POST: OrderDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int productId)
        {
            var orderDetails = await _context.OrderDetails.FindAsync(id, productId);
            _context.OrderDetails.Remove(orderDetails);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderDetailsExists(int id, int productId)
        {
            return _context.OrderDetails.Any(e => e.OrderId == id && e.ProductId == productId);
        }
    }
}
