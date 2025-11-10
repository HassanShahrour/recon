using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.Data.Models;
using Reconova.ViewModels.Tools;

namespace Reconova.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ToolController : Controller
    {
        private readonly IToolsRepository _toolsRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IPlanRepository _planRepository;

        public ToolController(IToolsRepository toolsRepository, ICategoryRepository categoryRepository, IPlanRepository planRepository)
        {
            _toolsRepository = toolsRepository;
            _categoryRepository = categoryRepository;
            _planRepository = planRepository;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var tools = await _toolsRepository.GetAllTools();
                var categories = await _categoryRepository.GetAllCategories();
                var plans = await _planRepository.GetAllPlans();

                if (!tools.IsSuccess || !categories.IsSuccess || !plans.IsSuccess)
                {
                    TempData["Error"] = "Failed to load tools, categories, or plans.";
                    return View(new ToolViewModel());
                }

                var model = new ToolViewModel
                {
                    Tools = tools.Value ?? new List<Tool>(),
                    Categories = categories.Value ?? new List<Category>(),
                    Plans = plans.Value ?? new List<Plan>()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unexpected error loading tools: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [Route("api/tool")]
        public async Task<IActionResult> CreateToolApi([FromBody] Tool tool)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _toolsRepository.AddTool(tool);
                if (result.IsSuccess)
                    return Ok(tool);

                return StatusCode(500, result.Error ?? "Failed to add tool");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Unexpected error adding tool: {ex.Message}");
            }
        }

        [HttpPut]
        [Route("api/tool/{id}")]
        public async Task<IActionResult> EditToolApi(int id, [FromBody] Tool updatedTool)
        {
            try
            {
                var existing = await _toolsRepository.GetToolById(id);
                if (!existing.IsSuccess || existing.Value == null)
                    return NotFound("Tool not found");

                updatedTool.Id = id;
                var result = await _toolsRepository.UpdateTool(updatedTool);
                if (result.IsSuccess)
                    return Ok(updatedTool);

                return StatusCode(500, result.Error ?? "Failed to update tool");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Unexpected error updating tool: {ex.Message}");
            }
        }

        [HttpDelete]
        [Route("api/tool/{id}")]
        public async Task<IActionResult> DeleteToolApi(int id)
        {
            try
            {
                var result = await _toolsRepository.DeleteTool(id);
                if (result.IsSuccess)
                    return NoContent();

                return StatusCode(500, result.Error ?? "Failed to delete tool");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Unexpected error deleting tool: {ex.Message}");
            }
        }
    }
}
