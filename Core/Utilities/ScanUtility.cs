using Microsoft.Extensions.Options;
using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.Settings;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Reconova.Core.Utilities
{
    public class ScanUtility
    {
        private readonly IScanRepository _scanRepository;
        private readonly string? _apiKey;
        private readonly string? _apiUrl;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public ScanUtility(IOptions<OpenRouterSettings> options, IScanRepository scanRepository)
        {
            _apiKey = options.Value.ApiKey;
            _apiUrl = options.Value.ApiUrl;
            _scanRepository = scanRepository;
        }

        public async Task<string> StartReconScanAsync(string target, string tool, int taskId)
        {
            var scanId = Guid.NewGuid().ToString();
            var command = $"{tool} {target}";

            string output;

            try
            {
                output = await RunProcessAsync("wsl", command);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to run command '{command}': {ex.Message}");
                return scanId;
            }

            if (string.IsNullOrWhiteSpace(output))
            {
                Console.WriteLine($"[WARNING] No output returned from command: {command}");
                return scanId;
            }

            var userPrompt = $"{output}\nAnalyze these results, interpret if there are any vulnerabilities, suggest mitigations if there are vulnerabilities or misconfigurations" +
                $", suggest some commands for further scanning, and draw the road map.";

            var requestPayload = new
            {
                model = "mistralai/mistral-7b-instruct:free",
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful assistant." },
                    new { role = "user", content = userPrompt }
                }
            };

            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var content = new StringContent(
                    JsonSerializer.Serialize(requestPayload, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync(_apiUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[ERROR] API request failed ({response.StatusCode}): {error}");
                    return scanId;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(jsonResponse);

                var reply = doc.RootElement
                               .GetProperty("choices")[0]
                               .GetProperty("message")
                               .GetProperty("content")
                               .GetString();

                await _scanRepository.AddScan(scanId, taskId, target, command, output, reply ?? "No response");
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"[ERROR] Failed to parse JSON: {jsonEx.Message}");
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"[ERROR] HTTP request failed: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Unexpected error: {ex.Message}");
            }

            return scanId;
        }

        private async Task<string> RunProcessAsync(string command, string args)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = startInfo };
                process.Start();

                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                if (!string.IsNullOrWhiteSpace(error))
                    output += $"\nErrors: {error}";

                return output;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to execute process '{command} {args}': {ex.Message}");
                throw;
            }
        }

    }
}
