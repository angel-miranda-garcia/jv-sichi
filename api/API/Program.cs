using System.Text;
using Application.Auth;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION")
    ?? throw new InvalidOperationException("DB_CONNECTION environment variable is not set.");

builder.Services.AddDbContext<SichiDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString),
        mysqlOptions => mysqlOptions.MigrationsAssembly(typeof(SichiDbContext).Assembly.GetName().Name)));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET")!)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuditService, AuditService>();

var corsOrigins = (Environment.GetEnvironmentVariable("CORS_ORIGINS") ?? string.Empty)
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
{
    options.AddPolicy("SichiPolicy", policy =>
    {
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseMiddleware<API.Middleware.ErrorHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseCors("SichiPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SichiDbContext>();
    db.Database.Migrate();
    await SeedAdmin(db);
}

app.Run();

static async Task SeedAdmin(SichiDbContext db)
{
    var email = Environment.GetEnvironmentVariable("ADMIN_EMAIL")
        ?? throw new InvalidOperationException("ADMIN_EMAIL environment variable is not set.");
    var password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD")
        ?? throw new InvalidOperationException("ADMIN_PASSWORD environment variable is not set.");

    if (await db.Users.AnyAsync())
        return;

    db.Users.Add(new User
    {
        Email = email,
        PasswordHash = PasswordHasher.HashPassword(password),
        CreatedAt = DateTime.UtcNow
    });

    await db.SaveChangesAsync();
}
