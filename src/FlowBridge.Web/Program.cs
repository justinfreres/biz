using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using FlowBridge.Web.Contracts;
using FlowBridge.Web.Data;
using FlowBridge.Web.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
var isDevelopment = builder.Environment.IsDevelopment();
var databaseDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(databaseDirectory);
var databasePath = Path.Combine(databaseDirectory, "flowbridge.db");
var keyDirectory = new DirectoryInfo(Path.Combine(databaseDirectory, "keys"));
keyDirectory.Create();

builder.Services.AddDbContext<ContentDbContext>(options => options.UseSqlite($"Data Source={databasePath}"));
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(keyDirectory)
    .SetApplicationName("FlowBridge.Web");
builder.Services.AddHealthChecks();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/admin/login.html";
        options.AccessDeniedPath = "/admin/login.html";
        options.Cookie.Name = "FlowBridge.Admin";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = isDevelopment ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization(options =>
    options.AddPolicy("Administrator", policy => policy.RequireRole("Administrator")));
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "FlowBridge.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = isDevelopment ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("admin-login", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(5);
        limiterOptions.QueueLimit = 0;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var database = scope.ServiceProvider.GetRequiredService<ContentDbContext>();
    await database.Database.EnsureCreatedAsync();
    await ContentSeeder.SeedAsync(database);
    await database.Database.ExecuteSqlRawAsync("PRAGMA optimize;");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/index.html");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapHealthChecks("/health");
app.MapGet("/api/services", async (ContentDbContext database, CancellationToken cancellationToken) =>
{
    var services = await database.ServiceOptions
        .AsNoTracking()
        .Where(service => service.IsPublished)
        .OrderBy(service => service.SortOrder)
        .ThenBy(service => service.Id)
        .Select(service => ServiceOptionMapper.ToPublicDto(service))
        .ToListAsync(cancellationToken);

    return Results.Ok(services);
});

app.MapGet("/api/admin/antiforgery", (IAntiforgery antiforgery, HttpContext context) =>
{
    var tokens = antiforgery.GetAndStoreTokens(context);
    return Results.Ok(new AntiforgeryTokenResponse(tokens.RequestToken));
});

app.MapPost("/api/admin/login", async (AdminLoginRequest request, HttpContext context, IAntiforgery antiforgery) =>
{
    var antiforgeryFailure = await AntiforgeryGuard.ValidateAsync(antiforgery, context);
    if (antiforgeryFailure is not null)
    {
        return antiforgeryFailure;
    }

    var passwordEnvironmentVariable = app.Configuration["Admin:PasswordEnvironmentVariable"] ?? "FLOWBRIDGE_ADMIN_PASSWORD";
    var configuredPassword = Environment.GetEnvironmentVariable(passwordEnvironmentVariable);
    if (string.IsNullOrWhiteSpace(configuredPassword))
    {
        return Results.Problem(
            title: "Local administrator password is not configured",
            detail: $"Set {passwordEnvironmentVariable} before starting the application.",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    var suppliedPassword = request.Password ?? string.Empty;
    var suppliedBytes = Encoding.UTF8.GetBytes(suppliedPassword);
    var configuredBytes = Encoding.UTF8.GetBytes(configuredPassword);
    if (!CryptographicOperations.FixedTimeEquals(suppliedBytes, configuredBytes))
    {
        return Results.Unauthorized();
    }

    var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, "FlowBridge Administrator"),
            new Claim(ClaimTypes.Role, "Administrator")
        ],
        CookieAuthenticationDefaults.AuthenticationScheme);
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

    return Results.NoContent();
}).RequireRateLimiting("admin-login");

var adminApi = app.MapGroup("/api/admin")
    .RequireAuthorization("Administrator")
    .AddEndpointFilter(async (context, next) =>
    {
        if (!HttpMethods.IsGet(context.HttpContext.Request.Method))
        {
            var antiforgery = context.HttpContext.RequestServices.GetRequiredService<IAntiforgery>();
            var antiforgeryFailure = await AntiforgeryGuard.ValidateAsync(antiforgery, context.HttpContext);
            if (antiforgeryFailure is not null)
            {
                return antiforgeryFailure;
            }
        }

        return await next(context);
    });

adminApi.MapGet("/session", (ClaimsPrincipal user) =>
    Results.Ok(new AdminSessionResponse(user.Identity?.Name ?? "Administrator")));

adminApi.MapGet("/services", async (ContentDbContext database, CancellationToken cancellationToken) =>
{
    var services = await database.ServiceOptions
        .AsNoTracking()
        .OrderBy(service => service.SortOrder)
        .ThenBy(service => service.Id)
        .Select(service => ServiceOptionMapper.ToAdminDto(service))
        .ToListAsync(cancellationToken);

    return Results.Ok(services);
});

adminApi.MapPost("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.NoContent();
});

adminApi.MapPost("/services", async (ServiceOptionInput input, ContentDbContext database, CancellationToken cancellationToken) =>
{
    var errors = ServiceOptionMapper.Validate(input);
    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    var slugBase = ServiceOptionMapper.CreateSlug(input.Title!);
    var slug = slugBase;
    var suffix = 2;
    while (await database.ServiceOptions.AnyAsync(service => service.Slug == slug, cancellationToken))
    {
        slug = $"{slugBase}-{suffix++}";
    }

    var service = new ServiceOption { Slug = slug };
    ServiceOptionMapper.ApplyInput(service, input);
    database.ServiceOptions.Add(service);
    await database.SaveChangesAsync(cancellationToken);

    return Results.Created($"/api/admin/services/{service.Id}", ServiceOptionMapper.ToAdminDto(service));
});

adminApi.MapPut("/services/{id:int}", async (int id, ServiceOptionInput input, ContentDbContext database, CancellationToken cancellationToken) =>
{
    var errors = ServiceOptionMapper.Validate(input);
    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    var service = await database.ServiceOptions.FindAsync([id], cancellationToken);
    if (service is null)
    {
        return Results.NotFound();
    }

    ServiceOptionMapper.ApplyInput(service, input);
    await database.SaveChangesAsync(cancellationToken);
    return Results.Ok(ServiceOptionMapper.ToAdminDto(service));
});

adminApi.MapDelete("/services/{id:int}", async (int id, ContentDbContext database, CancellationToken cancellationToken) =>
{
    var service = await database.ServiceOptions.FindAsync([id], cancellationToken);
    if (service is null)
    {
        return Results.NotFound();
    }

    database.ServiceOptions.Remove(service);
    await database.SaveChangesAsync(cancellationToken);
    return Results.NoContent();
});

app.MapFallbackToFile("index.html");

app.Run();

public partial class Program;

internal static class AntiforgeryGuard
{
    public static async ValueTask<IResult?> ValidateAsync(IAntiforgery antiforgery, HttpContext context)
    {
        try
        {
            await antiforgery.ValidateRequestAsync(context);
            return null;
        }
        catch (AntiforgeryValidationException)
        {
            return Results.BadRequest(new { detail = "The secure request token was missing or invalid. Refresh the page and try again." });
        }
    }
}
