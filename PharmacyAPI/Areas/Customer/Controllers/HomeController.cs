using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmacyAPI.DataAccess;
using PharmacyAPI.DTOs.Response;
using PharmacyAPI.Models;
using PharmacyAPI.Repositories;
using PharmacyAPI.Utils;

namespace PharmacyAPI.Areas.Customer.Controllers
{
    [Authorize]
    [Area(CD.CUSTOMER_AREA)]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            IRepository<Category> categoryRepository,
            IRepository<Product> productRepository,
            IRepository<Order> orderRepository,
            UserManager<ApplicationUser> userManager)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "User is not authenticated."
                });
            }

            var categories = await _categoryRepository.GetAllAsync();

            var products = await _productRepository.GetAllAsync(
                includes:
                [
                    p => p.Category
                ]
            );

            var orders = await _orderRepository.GetAllAsync(
                filter: o => o.ApplicationUserId == userId
            );

            var response = new CustomerHomeResponse
            {
                Categories = categories
    .Select(c => new CategoryItemResponse
    {
        Id = c.Id,
        Name = c.Name
    })
    .ToList(),

                Products = products
        .Select(p => new ProductResponse
        {
            Id = p.Id,
            Name = p.Name,
            GenericName = p.GenericName,
            Price = p.Price,
            MinStockLevel = p.MinStockLevel,
            RequiresPrescription = p.RequiresPrescription,
            CategoryId = p.CategoryId
        })
        .ToList(),

                RecentOrders = orders
        .OrderByDescending(o => o.OrderDate)
        .Take(5)
        .Select(o => new OrderResponse
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            OrderDate = o.OrderDate,
            Status = o.Status,
            IsPaid = o.IsPaid,
            TotalAmount = o.TotalAmount,
            Discount = o.Discount,
            DeliveryFees = o.DeliveryFees,
            NetAmount = o.NetAmount,
            PaymentMethod = o.PaymentMethod,
            DeliveryAddress = o.DeliveryAddress,
            Notes = o.Notes,
            ApplicationUserId = o.ApplicationUserId
        })
        .ToList()
            };

            return Ok(new ApiResponse<CustomerHomeResponse>
            {
                IsSuccess = true,
                Message = "Customer home data retrieved successfully",
                Data = response
            });
        }
    }
}
