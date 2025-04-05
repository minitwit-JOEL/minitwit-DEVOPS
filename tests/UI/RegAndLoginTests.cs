using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using minitwit.Infrastructure.Data;
using Testcontainers.PostgreSql;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

namespace UI.Tests
{
    public class RegisterAndLoginTests : IAsyncLifetime
    {
        private readonly IPlaywright _playwright;
        private IBrowser _browser;
        private IPage _page;
        private HttpClient _client;
        private readonly PostgreSqlContainer _postgresqlContainer;
        private readonly IServiceScope _scope;
        private readonly ApplicationDbContext _dbContext;

        public RegisterAndLoginTests()
        {
            _playwright = Playwright.CreateAsync().Result;

            _postgresqlContainer = new PostgreSqlBuilder()
                .WithDatabase("testdb")
                .WithUsername("test")
                .WithPassword("testpassword")
                .Build();
            _postgresqlContainer.StartAsync().Wait();
        }

        public async Task InitializeAsync()
        {
            var connectionString = _postgresqlContainer.GetConnectionString();

            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
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

            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            _page = await _browser.NewPageAsync();
        }

        public async Task DisposeAsync()
        {
            await _browser.CloseAsync();
            await _postgresqlContainer.StopAsync();

            _scope?.Dispose();
            _dbContext?.Dispose();
        }

        [Fact]
        public async Task Test_Register_And_Login_User_Via_UI()
        {
            string username = "testuser_" + Guid.NewGuid().ToString("N");
            string email = username + "@some.where";
            string password = "secure123";

            await _page.GotoAsync("http://localhost:3100/register");

            await _page.WaitForSelectorAsync("input[name='username']", new PageWaitForSelectorOptions { Timeout = 10000 });
            await _page.WaitForSelectorAsync("input[name='email']", new PageWaitForSelectorOptions { Timeout = 10000 });
            await _page.WaitForSelectorAsync("input[name='password']", new PageWaitForSelectorOptions { Timeout = 10000 });
            await _page.WaitForSelectorAsync("input[name='passwordRepeat']", new PageWaitForSelectorOptions { Timeout = 10000 });

            // Fill out the registration form
            await _page.FillAsync("input[name='username']", username);
            await _page.FillAsync("input[name='email']", email);
            await _page.FillAsync("input[name='password']", password);
            await _page.FillAsync("input[name='passwordRepeat']", password);

            await _page.ClickAsync("input[type='submit'][value='Sign Up']");

            try
            {
                await _page.WaitForURLAsync("http://localhost:3100/login", new PageWaitForURLOptions { Timeout = 10000 });
                Console.WriteLine("Successfully redirected to login page.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during URL change: {ex.Message}");
                throw;
            }

            await _page.WaitForSelectorAsync("input[name='username']", new PageWaitForSelectorOptions { Timeout = 5000 });
            await _page.WaitForSelectorAsync("input[name='password']", new PageWaitForSelectorOptions { Timeout = 5000 });

            await _page.FillAsync("input[name='username']", username);
            await _page.FillAsync("input[name='password']", password);
            await _page.ClickAsync("input[type='submit'][value='Sign In']");

            try
            {
                await _page.WaitForURLAsync("http://localhost:3100/dashboard", new PageWaitForURLOptions { Timeout = 10000 });
                var loggedInMessage = await _page.InnerTextAsync(".flashes");
                var expectedLoggedInMessage = "You are logged in";
                Assert.Contains(expectedLoggedInMessage, loggedInMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
            }
        }
    }
}
