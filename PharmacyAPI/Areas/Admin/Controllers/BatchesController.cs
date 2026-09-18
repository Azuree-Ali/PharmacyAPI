using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PharmacyAPI.Models;
using PharmacyAPI.Repositories;
using PharmacyAPI.Utils;

namespace PharmacyAPI.Areas.Admin.Controllers
{
    [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE},{CD.PHARMACIST_ROLE}")]
    [Area(CD.ADMIN_AREA)]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class BatchesController : ControllerBase
    {
        private readonly IRepository<ProductBatch> _productbatchrepository;
        private readonly IRepository<Product> _productRepository;

        public BatchesController(IRepository<ProductBatch> productbatchrepository, IRepository<Product> productRepository)
        {
            _productbatchrepository = productbatchrepository;
            _productRepository = productRepository;
        }


        [HttpGet]
        public async Task<IActionResult> GetInfo()
        {
            var batches = await _productbatchrepository.GetAllAsync(
                includes: [b => b.Product]
            );

            var result = batches.Select(b => new BatchesResponse
            {
                Id = b.Id,
                ProductId = b.ProductId,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate,
                CostPrice = b.CostPrice,
                QuantityOnHand = b.QuantityOnHand,
                ProductName = b.Product?.Name
            });
            if(result is null || !result.Any())
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "No batches found"
                });
            }
            return Ok(new ApiResponse<IEnumerable<BatchesResponse>>()
            {
                IsSuccess = true,
                Message = "Batches retrieved successfully",
                Data = result
            });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var batch = await _productbatchrepository.GetOneAsync(
                filter: b => b.Id == id,
                includes: [b => b.Product]
            );

            if (batch == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Batch not found"
                });
            }

            var response = new BatchesResponse
            {
                Id = batch.Id,
                ProductId = batch.ProductId,
                BatchNumber = batch.BatchNumber,
                ExpiryDate = batch.ExpiryDate,
                CostPrice = batch.CostPrice,
                QuantityOnHand = batch.QuantityOnHand,
                ProductName = batch.Product?.Name
            };

            return Ok(new ApiResponse<BatchesResponse>
            {
                IsSuccess = true,
                Message = "Batch retrieved successfully",
                Data = response
            });
        }
        [HttpPost]
        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE}")]
        public async Task<IActionResult> Create(BatchesRequest batchesRequest)
        {
            var product = await _productRepository.GetOneAsync(
            filter: p => p.Id == batchesRequest.ProductId
             );
            if (product == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Product not found"
                });
            }

            var batch = new ProductBatch
            {
                ProductId = batchesRequest.ProductId,
                BatchNumber = batchesRequest.BatchNumber,
                ExpiryDate = batchesRequest.ExpiryDate,
                CostPrice = batchesRequest.CostPrice,
                QuantityOnHand = batchesRequest.QuantityOnHand
            };

            await _productbatchrepository.CreateAsync(batch);
            await _productbatchrepository.CommitAsync();
            var response = new BatchesResponse
            {
                Id = batch.Id,
                ProductId = batch.ProductId,
                ProductName = product.Name,
                BatchNumber = batch.BatchNumber,
                ExpiryDate = batch.ExpiryDate,
                CostPrice = batch.CostPrice,
                QuantityOnHand = batch.QuantityOnHand
            };
            return CreatedAtAction(nameof(GetOne), new { id = batch.Id }, new ApiResponse<BatchesResponse> 
            { 
                IsSuccess = true,
                Message = "Batch created successfully",
                Data = response 
            });

        }
        [HttpPut("{id}")]
        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE}")]
        public async Task<IActionResult> Update(int id, BatchesRequest batchesRequest)
        {
            var batch = await _productbatchrepository.GetOneAsync(
                filter: b => b.Id == id
            );

            if (batch == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Batch not found"
                });
            }

            var product = await _productRepository.GetOneAsync(
                filter: p => p.Id == batchesRequest.ProductId
            );

            if (product == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Product not found"
                });
            }

            batch.ProductId = batchesRequest.ProductId;
            batch.BatchNumber = batchesRequest.BatchNumber;
            batch.ExpiryDate = batchesRequest.ExpiryDate;
            batch.CostPrice = batchesRequest.CostPrice;
            batch.QuantityOnHand = batchesRequest.QuantityOnHand;

            _productbatchrepository.Update(batch);
            await _productbatchrepository.CommitAsync();

            var response = new BatchesResponse
            {
                Id = batch.Id,
                ProductId = batch.ProductId,
                ProductName = product.Name,
                BatchNumber = batch.BatchNumber,
                ExpiryDate = batch.ExpiryDate,
                CostPrice = batch.CostPrice,
                QuantityOnHand = batch.QuantityOnHand
            };

            return Ok(new ApiResponse<BatchesResponse>
            {
                IsSuccess = true,
                Message = "Batch updated successfully",
                Data = response
            });
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE}")]
        public async Task<IActionResult> Delete(int id)
        {
            var batch = await _productbatchrepository.GetOneAsync(
                filter: b => b.Id == id
            );

            if (batch == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Batch not found"
                });
            }

            _productbatchrepository.Delete(batch);
            await _productbatchrepository.CommitAsync();

            return Ok(new ApiResponse<object>
            {
                IsSuccess = true,
                Message = "Batch deleted successfully"
            });
        }

    }
}
