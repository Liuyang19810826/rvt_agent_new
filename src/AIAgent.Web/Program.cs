using AIAgent.Core.Models;
using AIAgent.Core.Services;
using AIAgent.Infrastructure.Data;
using AIAgent.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add MudBlazor
builder.Services.AddMudServices();

// Add HttpClient for API calls
builder.Services.AddHttpClient("AIAgentApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:5078/");
});

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("AIAgentApi"));

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
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<AIAgent.Web.Components.App>()
    .AddInteractiveServerRenderMode();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();
