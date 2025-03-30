using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using minitwit.Infrastructure.Data;
using Testcontainers.PostgreSql;
using Xunit;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly HttpClient _client;
    private readonly IServiceScope _scope;
    private readonly ApplicationDbContext _dbContext;
    private readonly PostgreSqlContainer _postgresqlContainer;

    public IntegrationTests(WebApplicationFactory<Program> fixture)
    {
        _postgresqlContainer = new PostgreSqlBuilder()
            .WithDatabase("testdb")
            .WithUsername("test")
            .WithPassword("testpassword")
            .Build();

        _postgresqlContainer.StartAsync().Wait();

        var connectionString = _postgresqlContainer.GetConnectionString();

        var factory = fixture.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }

                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseNpgsql(connectionString));

                var serviceProvider = services.BuildServiceProvider();
                using (var scope = serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    dbContext.Database.EnsureDeleted();
                    dbContext.Database.Migrate();
                }
            });
        });

        _client = factory.CreateClient();
        _scope = factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }
    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
        _scope.Dispose();
        _postgresqlContainer.StopAsync().Wait();
    }

    [Fact]
    public async Task Test_Register()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Username = "testuser",
            Password = "password123",
            ConfirmPassword = "password123",
            Email = "testuser@example.com"
        });

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Test_Login()
    {
        // Arrange
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Username = "testuser",
            Password = "password123",
            ConfirmPassword = "password123",
            Email = "testuser@example.com"
        });

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Username = "testuser",
            Password = "password123"
        });

        // Assert
        response.EnsureSuccessStatusCode();
    }

}
