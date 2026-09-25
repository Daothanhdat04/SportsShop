using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsShop.Data;
using SportsShop.Models;

namespace SportsShop.Controllers
{

    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Orders
        public async Task<IActionResult> Index(string searchString, string status, DateTime? fromDate, DateTime? toDate)
        {
            ViewBag.SearchString = searchString;
            ViewBag.Status = status;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;

            var ordersQuery = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                ordersQuery = ordersQuery.Where(o =>
                    o.CustomerName.Contains(searchString) ||
                    o.CustomerEmail.Contains(searchString) ||
                    o.CustomerPhone.Contains(searchString) ||
                    o.Id.ToString().Contains(searchString));
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status))
            {
                ordersQuery = ordersQuery.Where(o => o.Status == status);
            }

            // Lọc theo ngày
            if (fromDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.OrderDate.Date >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.OrderDate.Date <= toDate.Value.Date);
            }

            var orders = await ordersQuery
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // GET: Admin/Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Admin/Orders/UpdateStatus
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = status;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật trạng thái đơn hàng thành công!";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Admin/Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Admin/Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.Id == id);
            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa đơn hàng thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        // Export Excel (bonus feature)
        public async Task<IActionResult> ExportExcel(DateTime? fromDate, DateTime? toDate)
        {
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Where(o => (!fromDate.HasValue || o.OrderDate >= fromDate) && (!toDate.HasValue || o.OrderDate <= toDate))
                .ToListAsync();

            // TODO: Implement Excel export
            TempData["SuccessMessage"] = "Tính năng xuất Excel sẽ được bổ sung sau!";
            return RedirectToAction(nameof(Index));
        }
    }
}