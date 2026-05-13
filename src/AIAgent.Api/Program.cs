using AIAgent.Core.Models;
using AIAgent.Core.Services;
using AIAgent.Infrastructure.Data;
using AIAgent.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=aiagent.db"));

// Add Memory Config
builder.Services.AddSingleton(new MemoryConfig
{
    ChromaUrl = builder.Configuration["Memory:ChromaUrl"] ?? "http://localhost:8000",
    Mem0Url = builder.Configuration["Memory:Mem0Url"] ?? "http://localhost:8001",
    MaxHistoryItems = builder.Configuration.GetValue<int>("Memory:MaxHistoryItems", 50)
});

// Add HttpClient for Memory Service
builder.Services.AddHttpClient<MemoryService>();

// Add Services
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IMemoryService, MemoryService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IMonitoringService, MonitoringService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();
