using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    public class CategoriesController : ControllerBase
    {
        private readonly IRepository<Category> _categoryRepository;

        public CategoriesController(IRepository<Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }
        [HttpGet]
        public async Task<IActionResult> GetInfo()
        {
            var categories = await _categoryRepository.GetAllAsync();
            if(categories == null)
            {
                return NotFound(new ApiResponse<object>() { IsSuccess = false, Message = "No categories found" });
            }
            return Ok(new ApiResponse<CategoryResponse>() { IsSuccess = true, Message = "Categories retrieved successfully", Data = new CategoryResponse() { Categories = categories } });
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var category = await _categoryRepository.GetOneAsync(filter: c => c.Id == id);
            if (category == null)
            {
                return NotFound(new ApiResponse<object>() { IsSuccess = false, Message = "Category not found" });
            }
            return Ok(new ApiResponse<CategoryResponse>() { IsSuccess = true, Message = "Category retrieved successfully", Data = new CategoryResponse() { Categories = new List<Category> { category } } });
        }

        [HttpPost]
        [Authorize(Roles = $" {CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE}")]

        public async Task<IActionResult> Create(CreateCategoryRequest createCategoryRequest)
        {
            var category = createCategoryRequest.Adapt<Category>();
            await _categoryRepository.CreateAsync(category);
            await _categoryRepository.CommitAsync();

            return CreatedAtAction(nameof(GetOne), new { id = category.Id }, new ApiResponse<object>() { IsSuccess = true, Message = "Category created successfully" } );
        }




        [HttpPut("{id}")]
        [Authorize(Roles = $" {CD.SUPER_ADMIN_ROLE}")]

        public async Task<IActionResult> Edit(int id, CreateCategoryRequest createCategoryRequest)
        {

            var category = await _categoryRepository.GetOneAsync(filter: c => c.Id == id);
            if (category == null)
            {
                return NotFound(new ApiResponse<object>() { IsSuccess = false, Message = "Category not found" });
            }

            category.Name = createCategoryRequest.Name;

            _categoryRepository.Update(category);
            await _categoryRepository.CommitAsync();

            return Ok(new ApiResponse<object>() { IsSuccess = true, Message = "Category updated successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $" {CD.SUPER_ADMIN_ROLE}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetOneAsync(filter: c => c.Id == id);
            if (category == null)
            {
                return NotFound(new ApiResponse<object>() { IsSuccess = false, Message = "Category not found" });
            }

            _categoryRepository.Delete(category);
            await _categoryRepository.CommitAsync();

            return Ok(new ApiResponse<object>() { IsSuccess = true, Message = "Category deleted successfully" });
        }
    }
}
