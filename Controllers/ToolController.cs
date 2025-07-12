using Microsoft.AspNetCore.Mvc;
using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.Data.Models;
using Reconova.ViewModels.Tools;

namespace Reconova.Controllers
{
    public class ToolController : Controller
    {

        public readonly IToolsRepository _toolsRepository;
        public readonly ICategoryRepository _categoryRepository;

        public ToolController(IToolsRepository toolsRepository, ICategoryRepository categoryRepository)
        {
            _toolsRepository = toolsRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index()
        {
            var tools = await _toolsRepository.GetAllTools();

            var categories = await _categoryRepository.GetAllCategories();

            var model = new ToolViewModel
            {
                Tools = tools.Value ?? new List<Tool>(),
                Categories = categories.Value ?? new List<Category>(),
            };

            return View(model ?? new ToolViewModel());
        }

        public IActionResult Add()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Tool tool)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Add));
            }

            try
            {
                var result = await _toolsRepository.AddTool(tool);
                if (result.IsSuccess)
                    TempData["Success"] = "Tool added successfully.";
                else
                    TempData["Error"] = "Error while adding tool";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unexpected error: {ex.Message}";
                return RedirectToAction(nameof(Add));
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var tool = await _toolsRepository.GetToolById(id);
            if (!tool.IsSuccess || tool.Value == null)
            {
                TempData["Error"] = "Tool not found.";
                return RedirectToAction(nameof(Index));
            }

            var categories = await _categoryRepository.GetAllCategories();

            var model = new ToolViewModel
            {
                Categories = categories.Value,
            };

            return View(model ?? new ToolViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Tool tool)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Edit));
            }

            try
            {
                var result = await _toolsRepository.UpdateTool(tool);
                if (result.IsSuccess)
                    TempData["Success"] = "Tool updated successfully.";
                else
                    TempData["Error"] = "Error while updating tool";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unexpected error: {ex.Message}";
                return RedirectToAction(nameof(Edit));
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _toolsRepository.DeleteTool(id);
                if (result.IsSuccess)
                    TempData["Success"] = "Tool deleted successfully.";
                else
                    TempData["Error"] = "Error while deleting tool";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unexpected error: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
