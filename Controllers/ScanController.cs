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
        private readonly AIUtility _openAI;

        public ScanController(IScanRepository scanRepository, ScanUtility scanUtility, ICategoryRepository categoryRepository, IToolsRepository toolsRepository, AIUtility openAI)
        {
            _scanRepository = scanRepository;
            _scanUtility = scanUtility;
            _categoryRepository = categoryRepository;
            _toolsRepository = toolsRepository;
            _openAI = openAI;
        }

        public async Task<IActionResult> Index()
        {
            var scans = await _scanRepository.GetAllScanResults();
            var categories = await _categoryRepository.GetAllCategories();
            var tools = await _toolsRepository.GetAllTools();

            var model = new ScanViewModel
            {
                ScanResults = scans.Value ?? new List<ScanResult>(),
                Categories = categories.Value ?? new List<Category>(),
                Tools = tools.Value ?? new List<Tool>(),
            };

            return View(model ?? new ScanViewModel());
        }

        public async Task<IActionResult> ScanResult(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    TempData["ErrorMessage"] = "Invalid scan Id.";
                    return RedirectToAction(nameof(Index));
                }

                var scanOutput = await _scanRepository.GetScanResultById(id);

                if (scanOutput == null)
                {
                    TempData["ErrorMessage"] = "Scan not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(scanOutput.Value ?? new ScanResult());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while fetching scan: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> StartScan(string target, string tool)
        {
            try
            {
                var scanId = await _scanUtility.StartReconScanAsync(target, tool);

                return RedirectToAction("ScanResult", new { id = scanId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while scanning: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> DownloadScanResult(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    TempData["ErrorMessage"] = "Invalid scan ID.";
                    return RedirectToAction(nameof(Index));
                }

                var scanOutput = await _scanRepository.GetScanResultById(id);

                if (scanOutput == null || scanOutput.Value == null)
                {
                    TempData["ErrorMessage"] = "Scan result not found.";
                    return RedirectToAction(nameof(Index));
                }

                var content = scanOutput.Value.Output ?? "No scan result available.";
                var fileName = $"Scan_{id}.txt";
                var contentType = "text/plain";
                var fileBytes = System.Text.Encoding.UTF8.GetBytes(content);

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error downloading file: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult Result()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Analyze()
        {
            //string filePath = @"C:\path\to\your-scan-output.txt";
            //if (!System.IO.File.Exists(filePath))
            //    return View("Error", "Scan file not found.");

            //string scanContent = await System.IO.File.ReadAllTextAsync(filePath);
            string analysis = await _openAI.AnalyzeScan("Starting Nmap 7.95 ( https://nmap.org ) at 2025-05-09 08:58 EEST\r\nNmap scan report for dns.google (8.8.8.8)\r\nHost is up (0.0050s latency).\r\nNot shown: 999 filtered tcp ports (no-response)\r\nPORT    STATE SERVICE\r\n443/tcp open  https\r\n\r\nNmap done: 1 IP address (1 host up) scanned in 4.59 seconds\r\n ");

            ViewBag.Analysis = analysis;
            return View("Result");
        }



    }
}
