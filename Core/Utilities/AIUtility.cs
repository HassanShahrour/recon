using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace Reconova.Core.Utilities
{
    public class AIUtility
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _endpoint;
        private readonly string _deploymentName;

        public AIUtility(IConfiguration config)
        {
            _apiKey = config["AzureOpenAI:ApiKey"] ?? "";
            _endpoint = config["AzureOpenAI:Endpoint"] ?? "";
            _deploymentName = config["AzureOpenAI:DeploymentName"] ?? "gpt-35-turbo";

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
        }

        public async Task<string> AnalyzeScan(string scanContent)
        {
            var messages = new[]
            {
                new { role = "system", content = "You are a security expert. Analyze the following output." },
                new { role = "user", content = scanContent }
            };

            var body = new
            {
                messages,
                temperature = 0.2,
                max_tokens = 1000
            };

            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

            var url = $"{_endpoint}/openai/deployments/{_deploymentName}/chat/completions?api-version=2024-02-15-preview";
            var response = await _httpClient.PostAsync(url, content);

            var resultJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Azure OpenAI Error: {response.StatusCode} - {resultJson}");

            dynamic result = JsonConvert.DeserializeObject(resultJson);
            return result.choices[0].message.content;
        }
    }
}
