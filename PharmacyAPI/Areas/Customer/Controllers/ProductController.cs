using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public class ProductController : ControllerBase
    {
        private readonly IRepository<Product> _productRepository;

        public ProductController(IRepository<Product> productRepository)
        {
            _productRepository = productRepository;
        }

        // GET: api/Customer/Product
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productRepository.GetAllAsync(
                includes: [p => p.Category],
                IsTracking: false
            );

            if (products == null || !products.Any())
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "No products found."
                });
            }

            var response = products.Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                GenericName = p.GenericName,
                Price = p.Price,
                MinStockLevel = p.MinStockLevel,
                RequiresPrescription = p.RequiresPrescription,
                CategoryId = p.CategoryId
            }).ToList();

            return Ok(new ApiResponse<List<ProductResponse>>
            {
                IsSuccess = true,
                Message = "Products retrieved successfully.",
                Data = response
            });
        }

        // GET: api/Customer/Product/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productRepository.GetOneAsync(
                filter: p => p.Id == id,
                includes: [p => p.Category],
                IsTracking: false
            );

            if (product == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Product not found."
                });
            }

            var response = new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                GenericName = product.GenericName,
                Price = product.Price,
                MinStockLevel = product.MinStockLevel,
                RequiresPrescription = product.RequiresPrescription,
                CategoryId = product.CategoryId
            };

            return Ok(new ApiResponse<ProductResponse>
            {
                IsSuccess = true,
                Message = "Product retrieved successfully.",
                Data = response
            });
        }
    }
}