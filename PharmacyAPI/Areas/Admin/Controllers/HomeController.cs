using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyAPI.Enums;
using PharmacyAPI.DataAccess;
using PharmacyAPI.DTOs.Response;
using PharmacyAPI.Models;
using PharmacyAPI.Utils;

namespace PharmacyAPI.Areas.Admin.Controllers
{
    [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE},{CD.PHARMACIST_ROLE}")]
    [Area(CD.ADMIN_AREA)]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var dashboard = new AdminDashboardResponse
            {
                TotalUsers = await _context.Users.CountAsync(),

                TotalProducts = await _context.Products.CountAsync(),

                TotalProductBatches =
                    await _context.ProductBatches.CountAsync(),

                TotalCustomers =
                    await _context.Customers.CountAsync(),

                TotalOrders =
                    await _context.Orders.CountAsync(),

                TotalSalesInvoices =
                    await _context.SalesInvoices.CountAsync(),

                TotalSales =
                    await _context.SalesInvoices
                        .Select(x => (decimal?)x.NetAmount)
                        .SumAsync() ?? 0,

                PendingOrders =
                    await _context.Orders
                        .CountAsync(x =>
                            x.Status == OrderStatus.Pending),

                ProcessingOrders =
                    await _context.Orders
                        .CountAsync(x =>
                            x.Status == OrderStatus.Processing),

                CompletedOrders =
                    await _context.Orders
                        .CountAsync(x =>
                            x.Status == OrderStatus.Completed),

                CancelledOrders =
                    await _context.Orders
                        .CountAsync(x =>
                            x.Status == OrderStatus.Cancelled)
            };

            return Ok(new ApiResponse<AdminDashboardResponse>
            {
                IsSuccess = true,
                Message = "Dashboard data retrieved successfully",
                Data = dashboard
            });
        }
    }
}