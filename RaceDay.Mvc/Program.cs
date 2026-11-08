using RaceDay.Mvc.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// ---- Session: holds the signed-in user's id, role and the API session cookie ----
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "RaceDay.Mvc.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ---- Typed HttpClient for the RaceDay API (Part 2). The MVC app has no database access. ----
// The API base address comes from configuration so the same build works locally and in Docker
// (ApiSettings__BaseUrl environment variable).
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5100";
builder.Services.AddHttpClient<ApiClient>(client =>
    {
        client.BaseAddress = new Uri(apiBaseUrl.TrimEnd('/') + "/");
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        var handler = new HttpClientHandler
        {
            UseCookies = false,        // the API session cookie is kept in the MVC session instead
            AllowAutoRedirect = false
        };
        if (builder.Environment.IsDevelopment())
        {
            // Local development only: accept the ASP.NET dev certificate when pointing at https://localhost.
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        }
        return handler;
    });

var app = builder.Build();

// Friendly error pages everywhere (never a raw stack trace or exception message).
app.UseExceptionHandler("/Home/Error");
app.UseStatusCodePagesWithReExecute("/Home/HttpStatus", "?code={0}");

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

// Stop the browser caching signed-in pages so "Back" after logout cannot reveal them.
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        if (context.Response.ContentType?.StartsWith("text/html", StringComparison.OrdinalIgnoreCase) == true)
        {
            context.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
            context.Response.Headers.Pragma = "no-cache";
        }
        return Task.CompletedTask;
    });
    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
