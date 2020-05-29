using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NorthwindCustomers.Models;
using PagedList;
using ReflectionIT.Mvc.Paging;

namespace NorthwindCustomers.Controllers
{
    public class OrdersController : Controller
    {
        private readonly NorthwindContext _context;

        public OrdersController(NorthwindContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            var northwindContext = from o in _context.Orders
                                   join c in _context.Customers on o.CustomerId equals c.CustomerId
                                   join e in _context.Employees on o.EmployeeId equals e.EmployeeId
                                   join d in _context.OrderDetails on o.OrderId equals d.OrderId
                                   group d by new { o.OrderId, c.CompanyName, o.OrderDate, e.FirstName, o.ShipAddress, o.ShipCity, o.ShipCountry } into g
                                   select new OrderInfo()
                                   {
                                       OrderId = g.Key.OrderId,
                                       CustomerName = g.Key.CompanyName,
                                       OrderDate = g.Key.OrderDate,
                                       TotalCost = g.Sum(d => d.UnitPrice * d.Quantity),
                                       AssignedTo = g.Key.FirstName,
                                       ShipAddress = g.Key.ShipAddress,
                                       ShipCity = g.Key.ShipCity,
                                       ShipCountry = g.Key.ShipCountry
                                   };

            if (!string.IsNullOrEmpty(searchString))
            {
                northwindContext = northwindContext.Where(x => x.CustomerName.Contains(searchString)
                 || x.AssignedTo.Contains(searchString)
                 || x.ShipCountry.Contains(searchString)
                 || x.ShipCity.Contains(searchString));

                var searchModel = await PagingList.CreateAsync(northwindContext.OrderBy(c => c.CustomerName), northwindContext.Count(), page);
                return View(searchModel);
            }

            var orders = northwindContext.AsNoTracking().OrderByDescending(o => o.OrderDate);
            var model = await PagingList.CreateAsync(orders, 10, page);
            return View(model);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.ShipViaNavigation)
                .Include(o => o.OrderDetails).ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (orders == null)
            {
                return NotFound();
            }

            return View(orders);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId");
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FirstName");
            ViewData["ShipVia"] = new SelectList(_context.Shippers, "ShipperId", "CompanyName");
            ViewData["ProductID"] = new SelectList(_context.Products, "ProductId", "ProductName");
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,CustomerId,EmployeeId,OrderDate,RequiredDate,ShippedDate,ShipVia,Freight,ShipName,ShipAddress,ShipCity,ShipRegion,ShipPostalCode,ShipCountry,OrderDetails,ProductId,UnitPrice,Quantity,Discount")] OrderViewModel order)
        {
            if (ModelState.IsValid)
            {
                Orders o = new Orders {
                    OrderId = order.OrderId,
                    CustomerId = order.CustomerId,
                    EmployeeId = order.EmployeeId,
                    OrderDate = order.OrderDate,
                    RequiredDate = order.RequiredDate,
                    ShippedDate = order.ShippedDate,
                    ShipVia = order.ShipVia,
                    Freight = order.Freight,
                    ShipName = order.ShipName,
                    ShipAddress = order.ShipAddress,
                    ShipCity = order.ShipCity,
                    ShipRegion = order.ShipRegion,
                    ShipPostalCode = order.ShipPostalCode,
                    ShipCountry = order.ShipCountry
                };
                OrderDetails d = new OrderDetails
                {
                    OrderId = order.OrderId,
                    Order = o,
                    ProductId = order.ProductId,
                    UnitPrice = order.UnitPrice,
                    Quantity = order.Quantity,
                    Discount = order.Discount
                };
                _context.Add(o);
                _context.Add(d);
            
                await _context.SaveChangesAsync();                             
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));

            }
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductName");
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", order.CustomerId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FirstName", order.EmployeeId);
            ViewData["ShipVia"] = new SelectList(_context.Shippers, "ShipperId", "CompanyName", order.ShipVia);
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orders = await _context.Orders.FindAsync(id);
            if (orders == null)
            {
                return NotFound();
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", orders.CustomerId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FirstName", orders.EmployeeId);
            ViewData["ShipVia"] = new SelectList(_context.Shippers, "ShipperId", "CompanyName", orders.ShipVia);
            return View(orders);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,CustomerId,EmployeeId,OrderDate,RequiredDate,ShippedDate,ShipVia,Freight,ShipName,ShipAddress,ShipCity,ShipRegion,ShipPostalCode,ShipCountry")] Orders orders)
        {
            if (id != orders.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orders);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrdersExists(orders.OrderId))
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
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", orders.CustomerId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FirstName", orders.EmployeeId);
            ViewData["ShipVia"] = new SelectList(_context.Shippers, "ShipperId", "CompanyName", orders.ShipVia);
            return View(orders);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.ShipViaNavigation)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (orders == null)
            {
                return NotFound();
            }

            return View(orders);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orders = await _context.Orders.FindAsync(id);
            _context.Orders.Remove(orders);
            var orderDetails = from o in _context.OrderDetails where o.OrderId == id select o;
            foreach (var orderDet in orderDetails)
            {
                _context.OrderDetails.Remove(orderDet);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrdersExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }
    }
}
