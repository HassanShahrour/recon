using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.Core.Utilities;
using Reconova.Data.Models;
using Reconova.ViewModels.Scan;

namespace Reconova.Controllers
{
    [Authorize]
    public class ScanController : Controller
    {
        private readonly IScanRepository _scanRepository;
        private readonly ScanUtility _scanUtility;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IToolsRepository _toolsRepository;
        private readonly ITaskRepository _taskRepository;

        public ScanController(
            IScanRepository scanRepository,
            ScanUtility scanUtility,
            ICategoryRepository categoryRepository,
            IToolsRepository toolsRepository,
            ITaskRepository taskRepository)
        {
            _scanRepository = scanRepository;
            _scanUtility = scanUtility;
            _categoryRepository = categoryRepository;
            _toolsRepository = toolsRepository;
            _taskRepository = taskRepository;
        }

        public async Task<IActionResult> Tasks()
        {
            try
            {
                var tasks = await _taskRepository.GetAllTasks();
                return View(tasks.Value ?? new List<Tasks>());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Unexpected error loading tasks: {ex.Message}";
                return View(new List<Tasks>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTask(Tasks task)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Tasks));
            }

            try
            {
                var result = await _taskRepository.AddTask(task);
                if (result.IsSuccess)
                    TempData["Success"] = "Task added successfully.";
                else
                    TempData["Error"] = "Error while adding task";

                return RedirectToAction(nameof(Tasks));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unexpected error: {ex.Message}";
                return RedirectToAction(nameof(Tasks));
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditTask([FromForm] Tasks task)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid task data." });
            }

            try
            {
                var result = await _taskRepository.UpdateTask(task);
                if (result.IsSuccess)
                    return Ok(new { message = "Task updated successfully." });
                else
                    return BadRequest(new { message = "Error while updating task." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Unexpected error: {ex.Message}" });
            }
        }

        [HttpPost("Scan/DeleteTask/{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            try
            {
                var result = await _taskRepository.DeleteTask(id);
                if (result.IsSuccess)
                    return Ok(new { message = "Task deleted successfully." });
                else
                    return BadRequest(new { message = "Error while deleting task." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Unexpected error: {ex.Message}" });
            }
        }


        public async Task<IActionResult> Index(int id)
        {
            try
            {
                var scansResult = await _scanRepository.GetAllScanResults(id);
                var categoriesResult = await _categoryRepository.GetAllCategories();
                var toolsResult = await _toolsRepository.GetAllTools();
                var task = await _taskRepository.GetTaskById(id);

                var target = task.Value.Target ?? "";

                var model = new ScanViewModel
                {
                    TaskId = id,
                    Target = target,
                    ScanResults = scansResult.Value ?? new List<ScanResult>(),
                    Categories = categoriesResult.Value ?? new List<Category>(),
                    Tools = toolsResult.Value ?? new List<Tool>()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Unexpected error loading scan data: {ex.Message}";
                return RedirectToAction(nameof(Tasks));
            }
        }

        public async Task<IActionResult> ScanResult(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["ErrorMessage"] = "Invalid scan Id.";
                return RedirectToAction(nameof(Tasks));
            }

            try
            {
                var scanResult = await _scanRepository.GetScanResultById(id);

                if (scanResult == null || scanResult.Value == null)
                {
                    TempData["ErrorMessage"] = "Scan not found.";
                    return RedirectToAction(nameof(Tasks));
                }

                return View(scanResult.Value);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error fetching scan result: {ex.Message}";
                return RedirectToAction(nameof(Tasks));
            }
        }

        [HttpPost]
        public async Task<IActionResult> StartScan(string target, string tool, int taskId)
        {
            if (string.IsNullOrWhiteSpace(target) || string.IsNullOrWhiteSpace(tool) || taskId <= 0)
            {
                TempData["ErrorMessage"] = "Target, tool, and valid task ID must be provided.";
                return RedirectToAction(nameof(Index), new { id = taskId });
            }

            try
            {
                var scanId = await _scanUtility.StartReconScanAsync(target, tool, taskId);
                return RedirectToAction(nameof(ScanResult), new { id = scanId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error starting scan: {ex.Message}";
                return RedirectToAction(nameof(Index), new { id = taskId });
            }
        }

        public async Task<IActionResult> DownloadScanResult(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["ErrorMessage"] = "Invalid scan ID.";
                return RedirectToAction(nameof(Tasks));
            }

            try
            {
                var scanResult = await _scanRepository.GetScanResultById(id);

                if (scanResult == null || scanResult.Value == null)
                {
                    TempData["ErrorMessage"] = "Scan result not found.";
                    return RedirectToAction(nameof(Tasks));
                }

                var content = scanResult.Value.Output ?? "No scan result available.";
                var fileName = $"Scan_{id}.txt";
                var contentType = "text/plain";
                var fileBytes = System.Text.Encoding.UTF8.GetBytes(content);

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error downloading scan result: {ex.Message}";
                return RedirectToAction(nameof(Tasks));
            }
        }

    }
}
