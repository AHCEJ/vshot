using FastEndpoints;
using Mapster;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VirtualShot.Database;
using VirtualShot.Features.Party;
using VirtualShot.Mapping;
using VirtualShot.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddSingleton<PresenceService>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "vshot_auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
        // API-style responses instead of redirects for unauthorized SPA calls.
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization(options =>
    options.AddPolicy("authed", policy => policy.RequireAuthenticatedUser()));

// Dev fallback for direct cross-origin access from the Nuxt dev server.
builder.Services.AddCors(options =>
    options.AddPolicy("dev", policy =>
        policy.WithOrigins("http://localhost:5101")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()));

builder.Services.AddSignalR();

var app = builder.Build();

TypeAdapterConfig.GlobalSettings.Scan(typeof(Program).Assembly);

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseCors("dev");
app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();

app.MapHub<PartyHub>("/hubs/party").RequireAuthorization("authed");

app.Run();
