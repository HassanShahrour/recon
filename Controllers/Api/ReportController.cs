using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Mvc;
using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.Data.Models;
using Reconova.ViewModels.Scan;
using System.Text;

namespace Reconova.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IConverter _converter;

        private readonly IScanRepository _scanRepository;

        public ReportController(IConverter converter, IScanRepository scanRepository)
        {
            _converter = converter;
            _scanRepository = scanRepository;
        }


        [HttpPost("export")]
        public async Task<IActionResult> ExportScanResultsReport([FromBody] ScanReportRequest request)
        {
            try
            {
                var scanResults = await _scanRepository.GetAllScanResults(request.TaskId);

                if (scanResults == null || !scanResults.Value.Any())
                {
                    return BadRequest("No scan results found for the given criteria.");
                }

                var html = GenerateHtml(scanResults.Value);

                var doc = new HtmlToPdfDocument
                {
                    GlobalSettings = new GlobalSettings
                    {
                        ColorMode = ColorMode.Color,
                        Orientation = Orientation.Portrait,
                        PaperSize = PaperKind.A4,
                        Margins = new MarginSettings { Top = 0, Bottom = 0, Right = 0, Left = 0 }
                    },
                    Objects =
            {
                new ObjectSettings
                {
                    HtmlContent = html,
                    WebSettings = new WebSettings
                    {
                        DefaultEncoding = "utf-8",
                        LoadImages = true,
                        PrintMediaType = true,
                        EnableIntelligentShrinking = true
                    },
                    PagesCount = true,
                    FooterSettings = { Center = "Page [page] of [toPage]" }
                }
            }
                };

                byte[] pdfBytes = _converter.Convert(doc);

                var fileName = $"ScanResults_Report_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
                Response.Headers["Content-Disposition"] = $"attachment; filename=\"{fileName}\"";
                return new FileContentResult(pdfBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine("PDF Export Error: " + ex.Message);
                return StatusCode(500, "Internal server error while generating the PDF.");
            }
        }




        public string GenerateHtml(List<ScanResult> results)
        {
            var sb = new StringBuilder();

            var reportDate = DateTime.Now.ToString("f");

            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "icon_bg_blue.png");
            var imageBytes = System.IO.File.ReadAllBytes(imagePath);
            var base64 = Convert.ToBase64String(imageBytes);
            var imgSrc = $"data:image/png;base64,{base64}";

            sb.Append(@"
<head>
    <style>
        body {
            font-family: Arial, sans-serif;
            background-color: #f5f5f5;
            margin: 0;
            padding: 0;
        }

        .header {
            background-color: #003366;
            color: white;
            padding: 20px 40px;
        }

        .header-left {
            width: 100%;
        }

        .header-logo {
            width: 60px;
            height: 60px;
            float: left;
            margin-right: 15px;
        }

        .header-text {
            overflow: hidden;
            padding-top: 8px;
        }

        .header-title {
            font-size: 24px;
            font-weight: bold;
            margin: 0 0 5px 0;
        }

        .header-date {
            font-size: 14px;
            color: #d0d0d0;
            margin: 0;
        }

        .container {
            max-width: 900px;
            margin: auto;
            padding: 20px;
            padding-top: 0px;
        }

        .scan-card {
            background-color: #fff;
            padding: 30px;
            margin-bottom: 30px;
            border-radius: 10px;
            box-shadow: 0 2px 6px rgba(0,0,0,0.1);
        }

        h2 {
            text-align: center;
            color: #3366cc;
            margin: 40px 0 20px;
        }

        h3 {
            color: #333;
            margin-top: 20px;
        }

        p {
            margin: 8px 0;
            line-height: 1.5;
        }

        pre {
            background-color: #f4f4f4;
            padding: 15px;
            border-radius: 6px;
            white-space: pre-wrap;
            word-wrap: break-word;
        }

        .analysis {
            background-color: #eef7ff;
            padding: 15px;
            border-radius: 6px;
            margin-top: 15px;
            line-height: 1.6;
        }

        .timestamp {
            text-align: right;
            font-size: 14px;
            color: #888;
            margin-top: 20px;
        }

        .divider {
            border-top: 1px solid #ccc;
            margin: 40px 0;
        }
    </style>
</head>
<body>
    <div class='header'>
        <div class='header-left'>
            <img src='" + imgSrc + @"' class='header-logo' alt='Logo' />
            <div class='header-text'>
                <div class='header-title'>Reconova Report</div>
                <div class='header-date'>" + reportDate + @"</div>
            </div>
        </div>
    </div>
    <div class='container'>
       
");

            // Optional: show target for context
            sb.Append($"<p style='text-align: center;'><strong>Target:</strong> {results[0].Target}</p>");

            foreach (var result in results)
            {
                sb.Append("<div class='scan-card'>");

                sb.Append($"<p><strong>Command:</strong> <code>{result.Command}</code></p>");

                sb.Append("<h3>Scan Output</h3>");
                sb.Append($"<pre>{(string.IsNullOrWhiteSpace(result.Output) ? "No output available." : result.Output)}</pre>");

                if (result.AIResult != null && !string.IsNullOrWhiteSpace(result.AIResult.Output))
                {
                    sb.Append("<h3>AI Analysis</h3>");
                    sb.Append($"<div class='analysis'>{result.AIResult.Output}</div>");
                }

                sb.Append($"<div class='timestamp'>Timestamp: {result.Timestamp.ToLocalTime():f}</div>");

                sb.Append("</div>");
            }

            sb.Append("</div></body>");

            return sb.ToString();
        }



    }
}
