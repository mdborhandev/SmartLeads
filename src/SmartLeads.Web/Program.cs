using SmartLeads.Web;
using SmartLeads.Infrastructure;
using SmartLeads.Utilities;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Enable detailed errors and logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();
builder.Services.AddApplication();
builder.Services.AddUtilities(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "SmartLeads.Session";
});
builder.Services.AddHttpContextAccessor();

// Configure Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SmartLeads API",
        Version = "v1",
        Description = "SmartLeads - Lead Management System API"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartLeads API v1");
        c.RoutePrefix = "swagger";
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Add storage folder as static file provider (at solution root: /SmartLeads/storage/uploads)
var storageUploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "storage", "uploads");
Directory.CreateDirectory(storageUploadsPath); // Ensure directory exists

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        storageUploadsPath),
    RequestPath = "/storage/uploads"
});

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Map Auth controller routes explicitly
app.MapControllerRoute(
    name: "auth",
    pattern: "Auth/{action=Login}/{id?}",
    defaults: new { controller = "Auth" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Landing}/{id?}");

// Show startup information
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("===========================================");
logger.LogInformation("SmartLeads Application Started");
logger.LogInformation("Frontend UI:  http://localhost:5284");
logger.LogInformation("Swagger Docs: http://localhost:5284/swagger");
logger.LogInformation("Landing Page: http://localhost:5284/");
logger.LogInformation("Register:     http://localhost:5284/Auth/Register");
logger.LogInformation("Login:        http://localhost:5284/Auth/Login");
logger.LogInformation("===========================================");
logger.LogInformation("Press Ctrl+C to stop the application");

app.Run();
