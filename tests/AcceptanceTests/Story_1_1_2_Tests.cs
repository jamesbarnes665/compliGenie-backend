using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CompliGenie.Tests.AcceptanceTests
{
    public class Story_1_1_2_TenantMiddlewareTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public Story_1_1_2_TenantMiddlewareTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task AC1_API_Key_Extraction_From_Headers_Works()
        {
            var client = _factory.CreateClient();
            
            // First register a partner to get an API key
            var registration = new
            {
                companyName = "Test Company AC1",
                email = "test@testcompanyac1.com",
                website = "https://testcompanyac1.com",
                industry = "Legal"
            };

            var response = await client.PostAsJsonAsync("/api/partners/register", registration);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadAsAsync<dynamic>();
            var apiKey = (string)result.apiKey;
            
            // Test API key extraction
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/test");
            request.Headers.Add("X-API-Key", apiKey);
            
            var testResponse = await client.SendAsync(request);
            Assert.NotEqual(HttpStatusCode.Unauthorized, testResponse.StatusCode);
        }

        [Fact]
        public async Task AC2_Invalid_API_Keys_Return_401()
        {
            var client = _factory.CreateClient();
            
            // Test with invalid API key
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/test");
            request.Headers.Add("X-API-Key", "invalid_key_12345");
            
            var response = await client.SendAsync(request);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Invalid API Key", content);
        }

        [Fact]
        public async Task AC3_Tenant_Context_Available_Throughout_Request()
        {
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddScoped<ITenantContextVerifier, TenantContextVerifier>();
                });
            }).CreateClient();
            
            // Register and get API key
            var registration = new
            {
                companyName = "Context Test Company",
                email = "test@contexttest.com",
                website = "https://contexttest.com",
                industry = "Legal"
            };

            var regResponse = await client.PostAsJsonAsync("/api/partners/register", registration);
            var result = await regResponse.Content.ReadAsAsync<dynamic>();
            var apiKey = (string)result.apiKey;
            
            // Make request with API key
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/test/context");
            request.Headers.Add("X-API-Key", apiKey);
            
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            
            var contextResult = await response.Content.ReadAsAsync<dynamic>();
            Assert.NotNull(contextResult.tenantId);
        }

        [Fact]
        public async Task AC4_Parallel_Requests_Maintain_Isolation()
        {
            var client = _factory.CreateClient();
            
            // Register two partners
            var partner1 = new
            {
                companyName = "Parallel Test 1",
                email = "test@parallel1.com",
                website = "https://parallel1.com",
                industry = "Legal"
            };
            
            var partner2 = new
            {
                companyName = "Parallel Test 2",
                email = "test@parallel2.com",
                website = "https://parallel2.com",
                industry = "HR"
            };

            var reg1 = await client.PostAsJsonAsync("/api/partners/register", partner1);
            var reg2 = await client.PostAsJsonAsync("/api/partners/register", partner2);
            
            var apiKey1 = (string)(await reg1.Content.ReadAsAsync<dynamic>()).apiKey;
            var apiKey2 = (string)(await reg2.Content.ReadAsAsync<dynamic>()).apiKey;
            
            // Make 100 parallel requests with alternating API keys
            var tasks = new List<Task<HttpResponseMessage>>();
            for (int i = 0; i < 100; i++)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "/api/test");
                request.Headers.Add("X-API-Key", i % 2 == 0 ? apiKey1 : apiKey2);
                tasks.Add(client.SendAsync(request));
            }
            
            var responses = await Task.WhenAll(tasks);
            
            // All requests should succeed
            Assert.All(responses, r => Assert.NotEqual(HttpStatusCode.Unauthorized, r.StatusCode));
        }

        [Fact]
        public async Task AC5_Performance_Overhead_Under_5ms()
        {
            var client = _factory.CreateClient();
            
            // Register a partner
            var registration = new
            {
                companyName = "Performance Test",
                email = "test@perftest.com",
                website = "https://perftest.com",
                industry = "Legal"
            };

            var regResponse = await client.PostAsJsonAsync("/api/partners/register", registration);
            var apiKey = (string)(await regResponse.Content.ReadAsAsync<dynamic>()).apiKey;
            
            // Warm up
            for (int i = 0; i < 10; i++)
            {
                var warmupRequest = new HttpRequestMessage(HttpMethod.Get, "/api/test");
                warmupRequest.Headers.Add("X-API-Key", apiKey);
                await client.SendAsync(warmupRequest);
            }
            
            // Measure middleware overhead
            var measurements = new List<long>();
            for (int i = 0; i < 100; i++)
            {
                var sw = Stopwatch.StartNew();
                
                var request = new HttpRequestMessage(HttpMethod.Get, "/api/test");
                request.Headers.Add("X-API-Key", apiKey);
                
                await client.SendAsync(request);
                
                sw.Stop();
                measurements.Add(sw.ElapsedMilliseconds);
            }
            
            var avgOverhead = measurements.Average();
            Assert.True(avgOverhead < 5, $"Average middleware overhead {avgOverhead}ms exceeds 5ms limit");
        }
    }

    public interface ITenantContextVerifier
    {
        Guid? GetCurrentTenantId();
    }

    public class TenantContextVerifier : ITenantContextVerifier
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantContextVerifier(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? GetCurrentTenantId()
        {
            return _httpContextAccessor.HttpContext?.Items["TenantId"] as Guid?;
        }
    }
}
