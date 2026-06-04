using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TekkenStats.Application.Interfaces;
using TekkenStats.Application.Services;
using TekkenStats.Infrastructure.Data;
using TekkenStats.Infrastructure.Repositories;
using TekkenStats.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<TekkenStatsDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=tekkenstats.db"));

// ── Repositories ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IMatchRepository, MatchRepository>();
builder.Services.AddScoped<IPlayerCharacterStatsRepository, PlayerCharacterStatsRepository>();
builder.Services.AddScoped<IBookmarkRepository, BookmarkRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IIngestionStateRepository, IngestionStateRepository>();
// ── Application services ──────────────────────────────────────────────────────
builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<BookmarkService>();
builder.Services.AddScoped<AuthService>();

// ── wavu.wiki ingestion ───────────────────────────────────────────────────────
builder.Services.AddHttpClient<IWavuReplayIngestionService, WavuReplayIngestionService>(client =>
{
    client.BaseAddress = new Uri("https://wank.wavu.wiki");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "TekkenStats/1.0");
});
builder.Services.AddHostedService<ReplayPollingService>();

// ── JWT authentication ────────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key must be set in configuration");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

// ── Swagger ───────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TekkenStats API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ── CORS (adjust origins for your frontend) ───────────────────────────────────
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(p =>
        p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

// ── Auto-migrate & Data Patch on startup ──────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TekkenStatsDbContext>();
    db.Database.Migrate();

    // 🛠️ SAFE DATA PATCH: Fixes old rank names without deleting database records!
    try
    {
        var legacyGods = db.Players.Where(p => p.DanRank >= 30 && p.DanRank <= 37).ToList();
        if (legacyGods.Any())
        {
            foreach (var player in legacyGods)
            {
                player.CurrentRank = player.DanRank switch
                {
                    30 => "God of Destruction 1",
                    31 => "God of Destruction 2",
                    32 => "God of Destruction 3",
                    33 => "God of Destruction 4",
                    34 => "God of Destruction 5",
                    35 => "God of Destruction 6",
                    36 => "God of Destruction 7",
                    37 => "God of Destruction Infinite",
                    _  => player.CurrentRank
                };
            }
            db.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        // Fail-safe to ensure application still starts up if anything goes wrong
        Console.WriteLine($"Error running rank synchronization patch: {ex.Message}");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();