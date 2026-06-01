using System.Text;
using Application.Auth;
using Application.Media;
using Application.Player;
using Application.Playlists;
using Application.Screens;
using Infrastructure.Storage;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Realtime;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var serverVersion = new MySqlServerVersion(new Version(8, 0, 0));
builder.Services.AddDbContext<SichiDbContext>(options =>
    options.UseMySql(
        Environment.GetEnvironmentVariable("DB_CONNECTION")!,
        serverVersion
    )
);

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
builder.Services.AddScoped<IScreenRepository, ScreenRepository>();
builder.Services.AddScoped<GetApprovedScreensUseCase>();
builder.Services.AddScoped<GetPendingScreensUseCase>();
builder.Services.AddScoped<ApproveScreenUseCase>();
builder.Services.AddScoped<RejectScreenUseCase>();
builder.Services.AddScoped<UpdateScreenUseCase>();
builder.Services.AddScoped<DeleteScreenUseCase>();
builder.Services.AddScoped<IPlaylistRepository, PlaylistRepository>();
builder.Services.AddScoped<GetPlaylistsUseCase>();
builder.Services.AddScoped<GetPlaylistDetailUseCase>();
builder.Services.AddScoped<CreatePlaylistUseCase>();
builder.Services.AddScoped<UpdatePlaylistUseCase>();
builder.Services.AddScoped<DeletePlaylistUseCase>();
builder.Services.AddScoped<ReorderPlaylistUseCase>();
builder.Services.AddScoped<AddMediaToPlaylistUseCase>();
builder.Services.AddScoped<RemoveMediaFromPlaylistUseCase>();
builder.Services.AddScoped<AssignPlaylistToScreenUseCase>();
builder.Services.AddScoped<IStorageService, StorageService>();
builder.Services.AddScoped<IMediaRepository, MediaRepository>();
builder.Services.AddScoped<UploadMediaUseCase>();
builder.Services.AddScoped<GetMediaUsageUseCase>();
builder.Services.AddScoped<DeleteMediaUseCase>();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 52_428_800;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 52_428_800;
});

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

builder.Services.AddSignalR();
builder.Services.AddScoped<IRealtimeService, RealtimeService>();
builder.Services.AddScoped<ForceRefreshUseCase>();
builder.Services.AddScoped<RegisterScreenUseCase>();
builder.Services.AddScoped<SyncPlaylistUseCase>();
builder.Services.AddScoped<GetPlayerVersionUseCase>();
builder.Services.AddScoped<PlayerHeartbeatUseCase>();

builder.Services.AddHostedService<HeartbeatMonitor>();
builder.Services.AddHostedService<MediaCleanup>();

var app = builder.Build();

app.UseMiddleware<API.Middleware.ErrorHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseCors("SichiPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<API.Middleware.UploadRateLimitMiddleware>();
app.MapControllers();
app.MapHub<PlayerHub>("/signalr/player");

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
        Role = "Admin",
        CreatedAt = DateTime.UtcNow
    });

    await db.SaveChangesAsync();
}
