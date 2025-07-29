using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace CompliGenie.Services
{
    public interface IPromptService
    {
        string LoadPrompt(string filename);
    }

    public class PromptService : IPromptService
    {
        private readonly IWebHostEnvironment _environment;
        
        public PromptService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }
        
        public string LoadPrompt(string filename)
        {
            var path = Path.Combine(_environment.ContentRootPath, "prompts", filename);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Prompt file not found: {filename}");
            }
            return File.ReadAllText(path);
        }
    }
}