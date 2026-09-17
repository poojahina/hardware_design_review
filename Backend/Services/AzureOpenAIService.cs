using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Backend.Models;

namespace Backend.Services;

public class AzureOpenAIService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public AzureOpenAIService(HttpClient httpClient, IConfiguration configuration, IWebHostEnvironment environment)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _environment = environment;
    }

    public async Task<string> AskAsync(string systemPrompt, object input)
    {
        var endpoint = _configuration["AzureOpenAI:Endpoint"];
        var apiKey = _configuration["AzureOpenAI:ApiKey"];
        var deploymentName = _configuration["AzureOpenAI:DeploymentName"];

        if (string.IsNullOrWhiteSpace(endpoint) ||
            string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(deploymentName))
        {
            return "{\"mode\":\"mock\",\"note\":\"Azure OpenAI configuration is empty, so deterministic POC reasoning was used.\"}";
        }

        var requestUri = $"{endpoint.TrimEnd('/')}/openai/deployments/{deploymentName}/chat/completions?api-version=2024-02-15-preview";
        var payload = new
        {
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = JsonSerializer.Serialize(input) }
            },
            temperature = 0.1,
            response_format = new { type = "json_object" }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        request.Headers.Add("api-key", apiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);

        return document.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "{}";
    }

    public async Task<ReviewResult> AnalyzeSchematicImageAsync(IFormFile image, HardwareDesign referenceDesign)
    {
        var endpoint = _configuration["AzureOpenAI:Endpoint"];
        var apiKey = _configuration["AzureOpenAI:ApiKey"];
        var deploymentName = _configuration["AzureOpenAI:DeploymentName"];

        if (string.IsNullOrWhiteSpace(endpoint) ||
            string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(deploymentName))
        {
            throw new InvalidOperationException("Azure OpenAI configuration is missing. Set AzureOpenAI:Endpoint, AzureOpenAI:ApiKey, and AzureOpenAI:DeploymentName.");
        }

        await using var imageStream = image.OpenReadStream();
        using var memoryStream = new MemoryStream();
        await imageStream.CopyToAsync(memoryStream);

        var base64Image = Convert.ToBase64String(memoryStream.ToArray());
        var mediaType = string.IsNullOrWhiteSpace(image.ContentType) ? "image/png" : image.ContentType;
        var imageUrl = $"data:{mediaType};base64,{base64Image}";
        var requestUri = BuildResponsesRequestUri(endpoint);

        var rulesText = await LoadRulesTextAsync();
        var prompt = """
            Analyze the uploaded hardware schematic image against the supplied engineering rules.

            Return JSON only using this exact shape:
            {
              "overallStatus": "PASS | DESIGN_ISSUES_DETECTED",
              "score": 0,
              "summary": "short summary",
              "metrics": {
                "components": 0,
                "nets": 0,
                "datasheets": 0,
                "findings": 0,
                "critical": 0,
                "high": 0,
                "medium": 0
              },
              "findings": [
                {
                  "component": "component reference",
                  "title": "finding title",
                  "issue": "what is wrong",
                  "severity": "CRITICAL | HIGH | MEDIUM",
                  "expected": "expected design/rule value",
                  "detected": "detected schematic value",
                  "evidence": "visible image evidence",
                  "rule": "matching rule code",
                  "reason": "why it violates the rule",
                  "recommendation": "fix recommendation",
                  "traceability": [
                    { "stage": "Image", "detail": "visible schematic evidence" },
                    { "stage": "Rule", "detail": "rule applied" },
                    { "stage": "Review", "detail": "severity decision" }
                  ]
                }
              ]
            }

            Use the plain-text rules supplied by the application as the authority for the review.
            """;

        var payload = new
        {
            model = deploymentName,
            input = new object[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new
                        {
                            type = "input_text",
                            text = $"You are a senior hardware design review engineer. You inspect circuit schematics and return strict JSON only.\n\n{prompt}\n\nPlain-text hardware review rules:\n{rulesText}\n\nReference datasheet constraints:\n{JsonSerializer.Serialize(referenceDesign.Datasheets)}"
                        },
                        new
                        {
                            type = "input_image",
                            image_url = imageUrl,
                            detail = "high"
                        }
                    }
                }
            }
        };

        Console.WriteLine($"Azure OpenAI endpoint: {requestUri}");
        Console.WriteLine($"Azure OpenAI deployment/model: {deploymentName}");

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        request.Headers.Add("api-key", apiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        using var response = await _httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"Azure OpenAI HTTP status: {(int)response.StatusCode} {response.StatusCode}");

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("Azure OpenAI Error:");
            Console.WriteLine(responseBody);

            throw new Exception(
                $"Azure OpenAI failed: {(int)response.StatusCode} " +
                $"{response.StatusCode}. Details: {responseBody}");
        }

        using var document = JsonDocument.Parse(responseBody);

        var content = ExtractResponsesOutputText(document.RootElement);

        var result = JsonSerializer.Deserialize<ReviewResult>(
            content ?? "{}",
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return result ?? throw new InvalidOperationException("Azure OpenAI returned an empty review result.");
    }

    private async Task<string> LoadRulesTextAsync()
    {
        var rulesPath = Path.Combine(_environment.ContentRootPath, "Rules", "hardware-review-rules.txt");

        if (!File.Exists(rulesPath))
        {
            throw new FileNotFoundException("Hardware review rules file was not found.", rulesPath);
        }

        return await File.ReadAllTextAsync(rulesPath);
    }

    private static string BuildResponsesRequestUri(string endpoint)
    {
        var baseEndpoint = endpoint.TrimEnd('/');
        var openAiPathIndex = baseEndpoint.IndexOf("/openai/", StringComparison.OrdinalIgnoreCase);

        if (openAiPathIndex >= 0)
        {
            baseEndpoint = baseEndpoint[..openAiPathIndex];
        }

        return $"{baseEndpoint}/openai/v1/responses";
    }

    private static string ExtractResponsesOutputText(JsonElement root)
    {
        if (root.TryGetProperty("output_text", out var outputText) &&
            outputText.ValueKind == JsonValueKind.String)
        {
            return outputText.GetString() ?? "{}";
        }

        if (!root.TryGetProperty("output", out var output) ||
            output.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("Azure OpenAI response did not contain an output array.");
        }

        foreach (var outputItem in output.EnumerateArray())
        {
            if (!outputItem.TryGetProperty("content", out var content) ||
                content.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var contentItem in content.EnumerateArray())
            {
                if (contentItem.TryGetProperty("text", out var text) &&
                    text.ValueKind == JsonValueKind.String)
                {
                    return text.GetString() ?? "{}";
                }
            }
        }

        throw new InvalidOperationException("Azure OpenAI response did not contain generated text.");
    }
}
