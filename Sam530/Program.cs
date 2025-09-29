using Sam530.Components;
using Sam530.Services;
using NLog;
using NLog.Web;




// Early init of NLog to allow startup and exception logging, before host is built
var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// NLog: Setup NLog for Dependency injection
builder.Logging.ClearProviders();
builder.Host.UseNLog();

//Services

builder.Services.AddSingleton<GenIOBridge>();
builder.Services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<GenIOBridge>());
builder.Services.AddSingleton<SyslogService>();
builder.Services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<SyslogService>());
builder.Services.AddSingleton<RadiusService>();
builder.Services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<RadiusService>());


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
