using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TrainersHub;
using TrainersHub.Data;
using TrainersHub.Middleware;
using TrainersHub.Models;
using TrainersHub.Models.Auth;
using TrainersHub.Services;

var builder = WebApplication.CreateBuilder(args);

// =====================================
// 1. Controllers
// =====================================
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true;
});

// =====================================
// 2. DB + DataProtection (для Render)
// =====================================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Ключи JWT/DataProtection храним в PostgreSQL, чтобы не сбрасывались при рестарте
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<AppDbContext>();

// =====================================
// 3. Password hasher + Strava
// =====================================
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddTransient<IStravaService, StravaService>();

// =====================================
// 4. JWT
// =====================================
var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtOptions>(jwtSection);

var jwtOptions = jwtSection.Get<JwtOptions>() ?? throw new Exception("Missing JWT config");
if (string.IsNullOrWhiteSpace(jwtOptions.Key))
    throw new Exception("JWT Key is missing in configuration");

var keyBytes = Encoding.UTF8.GetBytes(jwtOptions.Key);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// =====================================
// 5. Swagger
// =====================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =====================================
// 6. CORS
// =====================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("Cors", policy =>
    {
        policy.WithOrigins(
                "http://localhost:8080",
                "http://localhost:5173",
                "https://trainershub.onrender.com"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

// =====================================
// 7. Middleware pipeline
// =====================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Cors");

app.UseAuthentication();
app.UseAuthorization();

// Твой кастомный JWT middleware
app.UseWhen(context =>
    !context.Request.Path.StartsWithSegments("/api/trainer/CreateTraining"),
    appBuilder =>
    {
        appBuilder.UseJwtMiddleware();
    });

// Маршрутизация
app.MapControllers();

app.Run();
