using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.Data.Models;

namespace Reconova.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var categories = await _categoryRepository.GetAllCategories();
                return View(categories.Value ?? new List<Category>());
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unexpected error loading categories: {ex.Message}";
                return View(new List<Category>());
            }
        }

        [HttpGet("api/category/{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            try
            {
                var result = await _categoryRepository.GetCategoryById(id);
                if (!result.IsSuccess || result.Value == null)
                    return NotFound(new { message = "Category not found" });

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Unexpected error fetching category: {ex.Message}" });
            }
        }

        [HttpPost("api/category")]
        public async Task<IActionResult> AddCategory([FromBody] Category category)
        {
            if (category == null || string.IsNullOrWhiteSpace(category.Name))
                return BadRequest(new { message = "Invalid category data" });

            try
            {
                var result = await _categoryRepository.AddCategory(category);
                if (!result.IsSuccess)
                    return BadRequest(new { message = result.Error ?? "Error while adding category" });

                return Ok(new { message = "Category added successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Unexpected error adding category: {ex.Message}" });
            }
        }

        [HttpPut("api/category/{id}")]
        public async Task<IActionResult> EditCategory(int id, [FromBody] Category category)
        {
            if (category == null || id != category.Id || string.IsNullOrWhiteSpace(category.Name))
                return BadRequest(new { message = "Invalid category data" });

            try
            {
                var existing = await _categoryRepository.GetCategoryById(id);
                if (!existing.IsSuccess || existing.Value == null)
                    return NotFound(new { message = "Category not found" });

                var result = await _categoryRepository.UpdateCategory(category);
                if (!result.IsSuccess)
                    return BadRequest(new { message = result.Error ?? "Error while updating category" });

                return Ok(new { message = "Category updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Unexpected error updating category: {ex.Message}" });
            }
        }

        [HttpDelete("api/category/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var existing = await _categoryRepository.GetCategoryById(id);
                if (!existing.IsSuccess || existing.Value == null)
                    return NotFound(new { message = "Category not found" });

                var result = await _categoryRepository.DeleteCategory(id);
                if (!result.IsSuccess)
                    return BadRequest(new { message = result.Error ?? "Error while deleting category" });

                return Ok(new { message = "Category deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Unexpected error deleting category: {ex.Message}" });
            }
        }
    }
}
