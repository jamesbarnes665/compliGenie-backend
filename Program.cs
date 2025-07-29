using CompliGenie.Middleware;
using CompliGenie.Services;
using CompliGenie.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework with SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=CompliGenie.db"));

// Multi-Tenant Foundation Services
builder.Services.AddScoped<CompliGenie.Services.Interfaces.ITenantRepository, MockTenantRepository>();
builder.Services.AddScoped<CompliGenie.Services.Interfaces.ITenantService, MockTenantService>();
builder.Services.AddScoped<CompliGenie.Services.Interfaces.IStripeService, MockStripeService>();
builder.Services.AddScoped<CompliGenie.Services.Interfaces.IEmailService, MockEmailService>();

var app = builder.Build();

// Add tenant middleware for all API routes except registration
app.UseWhen(context => context.Request.Path.StartsWithSegments("/api") && 
                       !context.Request.Path.StartsWithSegments("/api/partners/register"),
            appBuilder => appBuilder.UseTenantMiddleware());

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Make Program accessible to tests
public partial class Program { }
