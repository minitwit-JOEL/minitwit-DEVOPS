
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using minitwit.Application.Interfaces;
using minitwit.Application.Interfaces.Sim;
using minitwit.Application.Services;
using minitwit.Application.Services.Sim;
using minitwit.Infrastructure.Data;
using AuthService = minitwit.Application.Services.AuthService;
using FollowService = minitwit.Application.Services.FollowService;
using IAuthService = minitwit.Application.Interfaces.IAuthService;
using IFollowService = minitwit.Application.Interfaces.IFollowService;
using ITwitsService = minitwit.Application.Interfaces.ITwitsService;
using TwitsService = minitwit.Application.Services.TwitsService;
using minitwit.Infrastructure.Dtos.Sim;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDistributedMemoryCache();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = false;
    options.Cookie.IsEssential = false;
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFollowService, FollowService>();
builder.Services.AddScoped<ITwitsService, TwitsService>();
builder.Services.AddScoped<IUserService, UserSerivce>();

// SIM api
builder.Services.AddScoped<minitwit.Application.Interfaces.Sim.IAuthService, minitwit.Application.Services.Sim.AuthService>();
builder.Services.AddScoped<minitwit.Application.Interfaces.Sim.IFollowService, minitwit.Application.Services.Sim.FollowService>();
builder.Services.AddScoped<minitwit.Application.Interfaces.Sim.ITwitsService, minitwit.Application.Services.Sim.TwitsService>();
builder.Services.AddScoped<ISimService, SimService>();

builder.Services.Configure<SimApiAccess>(builder.Configuration.GetSection("SimApiAccess"));

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "DevelopmentOrigins",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
<<<<<<< HEAD
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration.GetValue<string>("Token:Key"))),
            ValidIssuer = builder.Configuration.GetValue<string>("Token:Issuer"),
            ValidAudience = builder.Configuration.GetValue<string>("Token:Audience"),
=======
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("Token:Key").Value)),
            ValidIssuer = builder.Configuration.GetSection("Token:Issuer").Value, 
            ValidAudience = builder.Configuration.GetSection("Token:Audience").Value,
>>>>>>> main
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseCors("DevelopmentOrigins");
}
else
{
    app.UseCors("DevelopmentOrigins");
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Lax
});

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.Run();

public partial class Program { }