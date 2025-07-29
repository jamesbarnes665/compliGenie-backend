using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CompliGenie.Models;
using Polly;
using Polly.Extensions.Http;

namespace CompliGenie.Services
{
    public interface ILangChainService
    {
        Task<LLMResponse> Generate(GenerationRequest request);
    }

    public class LangChainService : ILangChainService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LangChainService> _logger;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;

        public LangChainService(
            HttpClient httpClient,
            ILogger<LangChainService> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = configuration["OpenAI:ApiKey"] ?? 
                Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? 
                throw new InvalidOperationException("OpenAI API key not configured");
            _baseUrl = configuration["OpenAI:BaseUrl"] ?? "https://api.openai.com/v1";
            
            // Configure retry policy
            _retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => !msg.IsSuccessStatusCode && msg.StatusCode != System.Net.HttpStatusCode.BadRequest)
                .WaitAndRetryAsync(
                    3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retry, context) =>
                    {
                        _logger.LogWarning("Retry {retry} after {timespan}s", retry, timespan.TotalSeconds);
                    });
        }

        public async Task<LLMResponse> Generate(GenerationRequest request)
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                var requestBody = new
                {
                    model = "gpt-4-turbo-preview",
                    messages = new[]
                    {
                        new { 
                            role = "system", 
                            content = "You are an AI compliance policy expert. Generate comprehensive, legally sound policies. Return a JSON object with 'Title' and 'Sections' array, where each section has 'Title' and 'Content' fields." 
                        },
                        new { 
                            role = "user", 
                            content = request.Prompt 
                        }
                    },
                    max_tokens = request.MaxTokens,
                    temperature = request.Temperature
                };

                var json = JsonSerializer.Serialize(requestBody);
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/chat/completions")
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
                httpRequest.Headers.Add("Authorization", $"Bearer {_apiKey}");

                var response = await _retryPolicy.ExecuteAsync(async () => 
                    await _httpClient.SendAsync(httpRequest.Clone())
                );
                
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("OpenAI API error: {StatusCode} - {Content}", 
                        response.StatusCode, responseContent);
                    
                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        throw new InvalidOperationException($"Bad request to OpenAI API: {responseContent}");
                    }
                    
                    throw new HttpRequestException($"OpenAI API error: {response.StatusCode}");
                }

                var apiResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

                return new LLMResponse
                {
                    Content = apiResponse.GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString() ?? string.Empty,
                    TokensUsed = apiResponse.GetProperty("usage")
                        .GetProperty("total_tokens")
                        .GetInt32(),
                    Duration = DateTime.UtcNow - startTime
                };
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request to OpenAI timed out");
                throw new TimeoutException("OpenAI API request timed out", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling OpenAI API");
                throw;
            }
        }
    }

    // Extension method to clone HttpRequestMessage
    public static class HttpRequestMessageExtensions
    {
        public static HttpRequestMessage Clone(this HttpRequestMessage req)
        {
            var clone = new HttpRequestMessage(req.Method, req.RequestUri);
            
            if (req.Content != null)
            {
                var content = req.Content.ReadAsStringAsync().Result;
                clone.Content = new StringContent(content, Encoding.UTF8, "application/json");
            }
            
            foreach (var header in req.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            
            return clone;
        }
    }
}