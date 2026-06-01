using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION")
    ?? throw new InvalidOperationException("DB_CONNECTION environment variable is not set.");

builder.Services.AddDbContext<SichiDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString),
        mysqlOptions => mysqlOptions.MigrationsAssembly(typeof(SichiDbContext).Assembly.GetName().Name)));

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
