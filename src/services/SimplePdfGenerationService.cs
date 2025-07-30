using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using CompliGenie.Models;
using CompliGenie.Context;

namespace CompliGenie.Services
{
    public class SimplePdfGenerationService : IPdfGenerationService
    {
        private readonly ILogger<SimplePdfGenerationService> _logger;
        private readonly ICurrentTenant _currentTenant;

        public SimplePdfGenerationService(
            ILogger<SimplePdfGenerationService> logger,
            ICurrentTenant currentTenant)
        {
            _logger = logger;
            _currentTenant = currentTenant;
        }

        public async Task<byte[]> GeneratePdf(PolicyDocument policy, TenantBranding branding)
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                _logger.LogInformation("Generating PDF for policy {PolicyId}", policy.Id);

                // Generate HTML directly
                var html = GenerateHtml(policy, branding);
                
                // Download Chromium if needed
                await new BrowserFetcher().DownloadAsync();
                
                using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
                });

                using var page = await browser.NewPageAsync();
                await page.SetContentAsync(html);
                
                var pdfOptions = new PdfOptions
                {
                    Format = PaperFormat.Letter,
                    PrintBackground = true,
                    MarginOptions = new MarginOptions
                    {
                        Top = "1in",
                        Bottom = "1in",
                        Left = "1in",
                        Right = "1in"
                    }
                };

                var pdf = await page.PdfDataAsync(pdfOptions);
                
                var duration = DateTime.UtcNow - startTime;
                _logger.LogInformation("PDF generated in {Duration}ms", duration.TotalMilliseconds);
                
                return pdf;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF");
                throw;
            }
        }

        private string GenerateHtml(PolicyDocument policy, TenantBranding branding)
        {
            branding ??= new TenantBranding
            {
                CompanyName = _currentTenant.Name ?? "CompliGenie Partner",
                PrimaryColor = "#0066CC",
                SecondaryColor = "#666666"
            };

            var toc = string.Join("\n", policy.Sections.OrderBy(s => s.Order).Select((s, i) => 
                $"<div style='margin-bottom: 10px;'>{i + 1}. {s.Title}</div>"));

            var sectionsHtml = string.Join("\n", policy.Sections.OrderBy(s => s.Order).Select((s, i) => 
                $@"<div style='page-break-before: {(i == 0 ? "avoid" : "always")};'>
                    <h1 style='color: {branding.PrimaryColor}; border-bottom: 2px solid {branding.PrimaryColor}; padding-bottom: 10px;'>{s.Title}</h1>
                    <div style='margin-top: 20px;'>{s.Content}</div>
                </div>"));

            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        @page {{
            size: letter;
            margin: 1in;
        }}
        
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            font-size: 11pt;
        }}
        
        .title-page {{
            text-align: center;
            page-break-after: always;
            padding-top: 100px;
        }}
        
        .title-page h1 {{
            font-size: 28pt;
            color: {branding.PrimaryColor};
            margin-bottom: 20px;
        }}
        
        .company-name {{
            font-size: 18pt;
            color: {branding.SecondaryColor};
            margin-bottom: 50px;
        }}
        
        .client-info {{
            margin-top: 100px;
            font-size: 14pt;
        }}
        
        .toc {{
            page-break-after: always;
        }}
        
        .toc h2 {{
            color: {branding.PrimaryColor};
            border-bottom: 2px solid {branding.PrimaryColor};
            padding-bottom: 10px;
        }}
        
        h1 {{
            color: {branding.PrimaryColor};
            font-size: 18pt;
        }}
        
        .footer {{
            text-align: center;
            margin-top: 50px;
            padding-top: 20px;
            border-top: 1px solid #ddd;
            color: #666;
            font-size: 9pt;
        }}
    </style>
</head>
<body>
    <!-- Title Page -->
    <div class='title-page'>
        {(string.IsNullOrEmpty(branding.LogoUrl) ? "" : $"<img src='{branding.LogoUrl}' style='max-height: 60px; margin-bottom: 20px;' />")}
        <h1>{policy.Title}</h1>
        <div class='company-name'>{branding.CompanyName}</div>
        
        <div class='client-info'>
            <p><strong>Prepared for:</strong></p>
            <p style='font-size: 16pt; font-weight: bold;'>{policy.ClientName}</p>
            <p style='margin-top: 20px;'>Date: {DateTime.UtcNow:MMMM d, yyyy}</p>
        </div>
    </div>
    
    <!-- Table of Contents -->
    <div class='toc'>
        <h2>Table of Contents</h2>
        {toc}
    </div>
    
    <!-- Policy Sections -->
    {sectionsHtml}
    
    <!-- Footer on last page -->
    <div class='footer'>
        <p>© {DateTime.UtcNow.Year} {branding.CompanyName}. All rights reserved.</p>
        <p>This document contains confidential and proprietary information.</p>
    </div>
</body>
</html>";
        }
    }
}