using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.Data.Models;

namespace Reconova.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepository.GetAllCategories();
            return View(categories.Value ?? new List<Category>());
        }


        public IActionResult Add()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Category category)
        {
            if (!ModelState.IsValid)
                return View(category);

            try
            {
                var result = await _categoryRepository.AddCategory(category);
                if (result.IsSuccess)
                    TempData["Success"] = "Category added successfully.";
                else
                    TempData["Error"] = "Error while adding category";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unexpected error: {ex.Message}";
                return View(category);
            }
        }


        public async Task<IActionResult> Edit(int id)
        {
            var result = await _categoryRepository.GetCategoryById(id);
            if (!result.IsSuccess || result.Value == null)
            {
                TempData["Error"] = "Category not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(result.Value);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category category)
        {
            if (!ModelState.IsValid)
                return View(category);

            try
            {
                var result = await _categoryRepository.UpdateCategory(category);
                if (result.IsSuccess)
                    TempData["Success"] = "Category updated successfully.";
                else
                    TempData["Error"] = "Error while updating category";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unexpected error: {ex.Message}";
                return View(category);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _categoryRepository.DeleteCategory(id);
                if (result.IsSuccess)
                    TempData["Success"] = "Category deleted successfully.";
                else
                    TempData["Error"] = "Error while deleting category";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unexpected error: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
