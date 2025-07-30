using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Reconova.Settings;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

[Route("api/ai")]
[ApiController]
public class AiController : ControllerBase
{

    private readonly string? _apiKey;
    private readonly string? _apiUrl;

    public AiController(IOptions<OpenRouterSettings> options)
    {
        _apiKey = options.Value.ApiKey;
        _apiUrl = options.Value.ApiUrl;
    }

    [HttpPost("suggested-tools")]
    public async Task<IActionResult> GetSuggestedTools([FromBody] AiPromptRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
                return BadRequest("Prompt cannot be empty.");

            var prompt = $"{request.Prompt}\nReturn only the list of tool names available on kali recommended for this task. No extra text. Limit to 8 maximum. Write them as bullets";

            var payload = new
            {
                model = "mistralai/mistral-7b-instruct:free",
                messages = new[]
                {
                    new { role = "system", content = "You are a cybersecurity assistant." },
                    new { role = "user", content = prompt }
                }
            };

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(_apiUrl, content);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"OpenRouter API Error: {errorText}");
                return StatusCode((int)response.StatusCode, "Error from AI service.");
            }

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            var reply = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            var tools = reply?
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim().TrimStart('-', '*', '•').Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList() ?? new List<string>();

            return Ok(new { tools });
        }
        catch (JsonException jsonEx)
        {
            Console.WriteLine($"JSON Parsing Error: {jsonEx.Message}");
            return StatusCode(500, "Failed to parse response from AI service.");
        }
        catch (HttpRequestException httpEx)
        {
            Console.WriteLine($"HTTP Request Error: {httpEx.Message}");
            return StatusCode(503, "Unable to reach AI service.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unhandled Error: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    public class AiPromptRequest
    {
        public string? Prompt { get; set; }
    }
}
