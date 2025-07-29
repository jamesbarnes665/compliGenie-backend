using CompliGenie.Middleware;
using CompliGenie.Services;
using CompliGenie.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add HttpContextAccessor for CurrentTenant
builder.Services.AddHttpContextAccessor();

// Add Entity Framework with SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=CompliGenie.db"));

// Multi-Tenant Foundation Services
builder.Services.AddScoped<CompliGenie.Context.ICurrentTenant, CompliGenie.Context.CurrentTenant>();
builder.Services.AddScoped<CompliGenie.Services.Interfaces.ITenantRepository, CompliGenie.Services.DbTenantRepository>();
builder.Services.AddScoped<CompliGenie.Services.Interfaces.ITenantService, MockTenantService>();
builder.Services.AddScoped<CompliGenie.Services.Interfaces.IStripeService, MockStripeService>();
builder.Services.AddScoped<CompliGenie.Services.Interfaces.IEmailService, MockEmailService>();

// Policy Generation Services
builder.Services.AddScoped<IPromptService, PromptService>();
builder.Services.AddScoped<ILangChainService, LangChainService>();
builder.Services.AddScoped<IPolicyGenerator, PolicyGenerator>();

// Add HttpClient for LangChainService
builder.Services.AddHttpClient<ILangChainService, LangChainService>();

var app = builder.Build();

// Add tenant middleware for all API routes except registration and health check
app.UseWhen(context => context.Request.Path.StartsWithSegments("/api") && 
                       !context.Request.Path.StartsWithSegments("/api/partners/register") &&
                       !context.Request.Path.StartsWithSegments("/api/healthcheck"),
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