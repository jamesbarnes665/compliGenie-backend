using Microsoft.EntityFrameworkCore;
using CompliGenie.Data;
using CompliGenie.Models;
using System;
using System.Threading.Tasks;

var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
optionsBuilder.UseSqlite("Data Source=CompliGenie.db");

using var context = new ApplicationDbContext(optionsBuilder.Options);

// Check if we already have a test tenant
var existingTenant = await context.Tenants.FirstOrDefaultAsync(t => t.Subdomain == "test");
if (existingTenant == null)
{
    // Create a test tenant
    var testTenant = new Tenant
    {
        Id = Guid.NewGuid(),
        Name = "Test Company",
        Subdomain = "test",
        ApiKeyHash = "test-api-key-123", // In production, this would be hashed
        StripeAccountId = null,
        Settings = "{}",
        CreatedAt = DateTime.UtcNow
    };

    context.Tenants.Add(testTenant);
    await context.SaveChangesAsync();
    
    Console.WriteLine($"Created test tenant:");
    Console.WriteLine($"  ID: {testTenant.Id}");
    Console.WriteLine($"  Name: {testTenant.Name}");
    Console.WriteLine($"  API Key: test-api-key-123");
}
else
{
    Console.WriteLine($"Test tenant already exists:");
    Console.WriteLine($"  ID: {existingTenant.Id}");
    Console.WriteLine($"  API Key: test-api-key-123");
}
