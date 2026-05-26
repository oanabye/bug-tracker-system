using System.Text;
using BugTracker.Server.Data;
using BugTracker.Server.Repositories;
using BugTracker.Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite("Data Source=bugtracker.db"));

// Repositories & Services
builder.Services.AddScoped<IBugRepository, BugRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBugService, BugService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IShareRequestRepository, ShareRequestRepository>();
builder.Services.AddScoped<IShareRequestService, ShareRequestService>();

// JWT Auth
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Auto-create DB on startup + seed admin
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    ctx.Database.EnsureCreated();
    ctx.Database.EnsureCreated();
    Console.WriteLine($"=== DB PATH: {ctx.Database.GetDbConnection().DataSource} ===");

    var userCount = ctx.Users.Count();
    Console.WriteLine($"=== Users in DB: {userCount} ===");

    if (!ctx.Users.Any())
    {
        Console.WriteLine("=== Seeding admin user ===");
        var hash = BCrypt.Net.BCrypt.HashPassword("admin123");
        Console.WriteLine($"=== Hash generated: {hash} ===");

        ctx.Users.Add(new BugTracker.Server.Models.User
        {
            Username = "admin",
            PasswordHash = hash,
            Role = BugTracker.Server.Models.UserRole.Administrator
        });

        ctx.Users.Add(new BugTracker.Server.Models.User
        {
            Username = "dev1",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("dev123"),
            Role = BugTracker.Server.Models.UserRole.Developer
        });
        var saved = ctx.SaveChanges();
        Console.WriteLine($"=== Rows saved: {saved} ===");
    }
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();