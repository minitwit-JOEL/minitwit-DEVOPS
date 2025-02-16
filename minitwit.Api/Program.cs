
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using minitwit.Application.Interfaces;
using minitwit.Application.Services;
using minitwit.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDistributedMemoryCache();
var connectionString = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=yourpassword;"
    ;//builder.Configuration.GetConnectionString("DbConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql("Host=localhost;Port=5433;Database=postgres;Username=root;Password=root;"));

builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = false;
    options.Cookie.IsEssential = false;
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFollowService, FollowService>();
builder.Services.AddScoped<ITwitsService, TwitsService>();
builder.Services.AddScoped<IUserService, UserSerivce>();

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "DevelopmentOrigins",
        policy =>
        {
            policy.WithOrigins("http://localhost:3100")
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("jg4mywvyYnFJgJhLT+6AmMllIL4t/86qY75kt42HRmV4=")),
            ValidateIssuer = false, 
            ValidateAudience = false,
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
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("DevelopmentOrigins");
app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Lax
});

app.MapControllers();
app.Run();

app.Run();