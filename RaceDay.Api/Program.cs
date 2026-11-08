using System.Reflection;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ---- Database (EF Core, Code-First, SQL Server) ----
// The real connection string lives in appsettings.Development.json (gitignored)
// or in user-secrets / an environment variable in production — never committed.
builder.Services.AddDbContext<RaceDayDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RaceDayDb")));

// ---- Server-side session (brief requires session-based auth state) ----
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "RaceDay.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllers();

// ---- Azure Blob Storage (Part 3) ----
// The connection string comes from user-secrets or the AzureBlob__ConnectionString
// environment variable (see README) — it is never committed.
builder.Services.Configure<AzureBlobOptions>(builder.Configuration.GetSection(AzureBlobOptions.SectionName));
builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>();

// ---- Swagger / OpenAPI ----
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "RaceDay API",
        Version = "v1",
        Description = "RESTful API for the RaceDay race and event management system. " +
                      "Register and log in as Participant or Organiser, then use the " +
                      "'Authorize' cookie session set by /api/auth/login for protected endpoints."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// ---- Seed roles + apply pending migrations on startup (dev convenience) ----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
    if (db.Database.IsRelational())
    {
        // Skipped for the in-memory provider used by the test project, which
        // doesn't support migrations.
        // Retries let the API start alongside a SQL Server container that is still booting.
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                if (db.Database.GetMigrations().Any())
                {
                    db.Database.Migrate();
                }
                else
                {
                    db.Database.EnsureCreated();
                }
                break;
            }
            catch (Exception) when (attempt < 10)
            {
                Thread.Sleep(TimeSpan.FromSeconds(3));
            }
        }
    }
    DbSeeder.SeedRoles(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "RaceDay API v1");
    });
}

// Disabled via configuration when running in Docker, where TLS is not terminated by the API.
if (!app.Configuration.GetValue<bool>("DisableHttpsRedirection"))
{
    app.UseHttpsRedirection();
}

app.UseSession(); // must run before controllers so HttpContext.Session is populated

app.MapControllers();

app.Run();

// Exposed so the integration-test WebApplicationFactory<Program> can find the entry point.
public partial class Program { }
