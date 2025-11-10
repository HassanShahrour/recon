using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.Core.Utilities;
using Reconova.Data.DTOs.Schedule;
using Reconova.Data.Models;
using Reconova.ViewModels.Scan;

namespace Reconova.Controllers
{
    [Authorize]
    public class ScanController : Controller
    {
        private readonly ScanUtility _scanUtility;
        private readonly UserUtility _userUtility;
        private readonly IScanRepository _scanRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IToolsRepository _toolsRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly IScheduleScansRepository _scheduleScansRepository;
        private readonly IUserRepository _userRepository;

        public ScanController(
            IScanRepository scanRepository,
            ScanUtility scanUtility,
            UserUtility userUtility,
            ICategoryRepository categoryRepository,
            IToolsRepository toolsRepository,
            ITaskRepository taskRepository,
            IScheduleScansRepository scheduleScansRepository,
            IUserRepository userRepository)
        {
            _scanRepository = scanRepository;
            _scanUtility = scanUtility;
            _categoryRepository = categoryRepository;
            _toolsRepository = toolsRepository;
            _taskRepository = taskRepository;
            _userUtility = userUtility;
            _scheduleScansRepository = scheduleScansRepository;
            _userRepository = userRepository;
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
                return RedirectToAction(nameof(Tasks));

            try
            {
                var result = await _taskRepository.AddTask(task);
                if (result.IsSuccess)
                    TempData["Success"] = "Task added successfully.";
                else
                    TempData["Error"] = result.Error ?? "Error while adding task";

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
                return BadRequest(new { message = "Invalid task data." });

            try
            {
                var result = await _taskRepository.UpdateTask(task);
                if (result.IsSuccess)
                    return Ok(new { message = "Task updated successfully." });

                return BadRequest(new { message = result.Error ?? "Error while updating task." });
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

                return BadRequest(new { message = result.Error ?? "Error while deleting task." });
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
                var scheduledScans = await _scheduleScansRepository.GetAllScheduledScans(id);

                var userId = await _userUtility.GetLoggedInUserId();
                var user = await _userRepository.GetUserById(userId.ToString() ?? "");

                var target = task.Value?.Target ?? "";

                var model = new ScanViewModel
                {
                    TaskId = id,
                    Target = target,
                    ScanResults = scansResult.Value ?? new List<ScanResult>(),
                    Categories = categoriesResult.Value ?? new List<Category>(),
                    Tools = toolsResult.Value ?? new List<Tool>(),
                    ScheduledScans = scheduledScans.Value ?? new List<ScheduledScan>()
                };

                // Nullable bool, safely check
                bool isAllowedToGenerateReport = user.Value?.Plan?.CanGenerateReport ?? false;
                ViewBag.IsAllowedToGenerateReport = isAllowedToGenerateReport;

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
                var userId = await _userUtility.GetLoggedInUserId();
                var canScan = await _scanRepository.CanUserScanToday();
                if (!canScan.Value)
                {
                    TempData["ErrorMessage"] = "You have reached your daily scan limit.";
                    return RedirectToAction(nameof(Index), new { id = taskId });
                }

                var scanId = await _scanUtility.StartReconScanAsync(userId.ToString() ?? "", target, tool, taskId);
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

        [HttpDelete]
        [Route("api/scan/{id}")]
        public async Task<IActionResult> DeleteScan(int id)
        {
            try
            {
                var result = await _scanRepository.DeleteScan(id);
                if (result.IsSuccess)
                    return NoContent();

                return StatusCode(500, result.Error ?? "Failed to delete scan");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Unexpected error deleting scan: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ScheduleScan(string Name, string Target, int TaskId, TimeOnly Time, List<string> ToolName)
        {
            try
            {
                var userId = await _userUtility.GetLoggedInUserId();

                var scheduledScan = new ScheduledScan
                {
                    Id = (int)(DateTime.UtcNow - new DateTime(2020, 1, 1)).TotalSeconds,
                    Name = Name,
                    UserId = userId.ToString(),
                    Target = Target,
                    Time = Time,
                    TaskId = TaskId,
                    IsActive = true
                };

                await _scheduleScansRepository.AddScheduledScan(scheduledScan);

                foreach (var tool in ToolName)
                {
                    var toolId = await _toolsRepository.GetToolIdByName(tool);

                    var scheduleTool = new ScheduledTool
                    {
                        ToolId = toolId.Value,
                        Tool = tool,
                        ScheduledScanId = scheduledScan.Id
                    };

                    await _scheduleScansRepository.AddScheduledScanTool(scheduleTool);
                }

                TempData["Success"] = "Scan scheduled successfully.";
                return RedirectToAction(nameof(Index), new { id = TaskId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error scheduling scan: {ex.Message}";
                return RedirectToAction(nameof(Index), new { id = TaskId });
            }
        }

        [HttpPut("api/schedules/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateScheduleDto dto)
        {
            if (id != dto.Id)
                return BadRequest("ID mismatch");

            try
            {
                var result = await _scheduleScansRepository.UpdateScheduleScan(dto);
                if (!result.IsSuccess)
                    return NotFound(result.Error);

                return Ok(new { message = "Schedule updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("api/schedules/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _scheduleScansRepository.DeleteScheduleScan(id);
                if (!result.IsSuccess)
                    return NotFound(result.Error);

                return Ok(new { message = "Scheduled scan deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
