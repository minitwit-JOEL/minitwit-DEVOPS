
using Microsoft.EntityFrameworkCore;
using minitwit.Application.Interfaces;
using minitwit.Application.Services;
using minitwit.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDistributedMemoryCache();
var connectionString = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=yourpassword;"
    ;//builder.Configuration.GetConnectionString("DbConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql("Host=localhost;Port=5433;Database=postgres;Username=postgres;Password=yourpassword;"));

builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFollowService, FollowService>();
builder.Services.AddScoped<ITwitsService, TwitsService>();

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

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseSession();
app.UseRouting();
app.UseCors("DevelopmentOrigins");
app.UseAuthorization();
app.MapControllers();
app.Run();

app.Run();