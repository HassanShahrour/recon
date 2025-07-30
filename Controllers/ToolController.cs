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
            try
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
            catch (Exception ex)
            {
                TempData["Error"] = $"Unexpected error: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [Route("api/tool")]
        public async Task<IActionResult> CreateToolApi([FromBody] Tool tool)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _toolsRepository.AddTool(tool);
            if (result.IsSuccess)
                return Ok(tool);

            return StatusCode(500, "Failed to add tool");
        }

        [HttpPut]
        [Route("api/tool/{id}")]
        public async Task<IActionResult> EditToolApi(int id, [FromBody] Tool updatedTool)
        {
            var existing = await _toolsRepository.GetToolById(id);
            if (!existing.IsSuccess || existing.Value == null)
                return NotFound("Tool not found");

            updatedTool.Id = id;

            var result = await _toolsRepository.UpdateTool(updatedTool);
            if (result.IsSuccess)
                return Ok(updatedTool);

            return StatusCode(500, "Failed to update tool");
        }

        [HttpDelete]
        [Route("api/tool/{id}")]
        public async Task<IActionResult> DeleteToolApi(int id)
        {
            var result = await _toolsRepository.DeleteTool(id);
            if (result.IsSuccess)
                return NoContent();

            return StatusCode(500, "Failed to delete tool");
        }


    }
}
