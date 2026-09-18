using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PharmacyAPI.Models;
using PharmacyAPI.Repositories;
using PharmacyAPI.Utils;

namespace PharmacyAPI.Areas.Admin.Controllers
{
    [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE},{CD.PHARMACIST_ROLE}")]
    //[Authorize(Roles = "SuperAdmin")]
    [Area(CD.ADMIN_AREA)]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IRepository<Product> _productrepository;
        private readonly IRepository<Category> _categoryRepository;
        public ProductsController(IRepository<Product> productrepository, IRepository<Category> categoryRepository)
        {
            _productrepository = productrepository;
            _categoryRepository = categoryRepository;
        }
        [HttpGet("get")]
        public async Task<IActionResult> GetInfo()
        {
            var products = await _productrepository.GetAllAsync(
                includes: [p => p.Category]
            );

            if (products == null || !products.Any())
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "No products found"
                });
            }

            var productResponses = products.Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                GenericName = p.GenericName,
                Price = p.Price,
                MinStockLevel = p.MinStockLevel,
                RequiresPrescription = p.RequiresPrescription,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name
            });

            return Ok(new ApiResponse<IEnumerable<ProductResponse>>
            {
                IsSuccess = true,
                Message = "Products retrieved successfully",
                Data = productResponses
            });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var product = await _productrepository.GetOneAsync(filter: c => c.Id == id);
            if (product == null)
            {
                return NotFound(new ApiResponse<object>() { IsSuccess = false, Message = "Product not found" });
            }
            return Ok(new ApiResponse<ProductFilterResponse>() { IsSuccess = true, Message = "Product retrieved successfully", Data = new ProductFilterResponse() { Products = new List<Product> { product } } });
        }
        [HttpPost]
        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE}")]
        public async Task<IActionResult> Create(CreateProductRequest request)
        {
            var category = await _categoryRepository.GetOneAsync(
                filter: c => c.Id == request.CategoryId
            );

            if (category == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Category not found"
                });
            }

            var product = new Product
            {
                Name = request.Name,
                GenericName = request.GenericName,
                Price = request.Price,
                MinStockLevel = request.MinStockLevel,
                RequiresPrescription = request.RequiresPrescription,
                CategoryId = request.CategoryId
            };

            await _productrepository.CreateAsync(product);
            await _productrepository.CommitAsync();

            return CreatedAtAction(
                nameof(GetOne),
                new { id = product.Id },
                new ApiResponse<object>
                {
                    IsSuccess = true,
                    Message = "Product created successfully"
                }
            );
        }
        [HttpPut("{id}")]
        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE}")]
        public async Task<IActionResult> Edit(
          int id,
          UpdateProductRequest updateProductRequest)
        {
            var product = await _productrepository.GetOneAsync(
                filter: p => p.Id == id
            );

            if (product == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Product not found"
                });
            }

            var category = await _categoryRepository.GetOneAsync(
                filter: c => c.Id == updateProductRequest.CategoryId
            );

            if (category == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Category not found"
                });
            }

            product.Name = updateProductRequest.Name;
            product.GenericName = updateProductRequest.GenericName;
            product.Price = updateProductRequest.Price;
            product.MinStockLevel = updateProductRequest.MinStockLevel;
            product.RequiresPrescription = updateProductRequest.RequiresPrescription;
            product.CategoryId = updateProductRequest.CategoryId;

            _productrepository.Update(product);
            await _productrepository.CommitAsync();

            return Ok(new ApiResponse<object>
            {
                IsSuccess = true,
                Message = "Product updated successfully"
            });
        }
        [HttpDelete]
        [Authorize(Roles = $" {CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE}")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _productrepository.GetOneAsync(filter: p => p.Id == id);
            if (product == null)
            {
                return NotFound(new ApiResponse<object>() { IsSuccess = false, Message = "Product not found" });
            }


            //if (!string.IsNullOrEmpty(product.ProductImg))
            //{
            //    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\", product.ProductImg);
            //    if (System.IO.File.Exists(imagePath))
            //    {
            //        System.IO.File.Delete(imagePath);
            //    }
            //}


            _productrepository.Delete(product);
            await _productrepository.CommitAsync();

            return Ok(new ApiResponse<object>() { IsSuccess = true, Message = "Product deleted successfully" });
        }
    }
}
