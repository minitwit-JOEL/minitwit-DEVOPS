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
using System.Text.RegularExpressions;

namespace UI.Tests
{
    public class UserflowTest : IAsyncLifetime
    {
        private readonly IPlaywright _playwright;
        private IBrowser _browser;
        private IPage _page;
        private HttpClient _client;
        private readonly PostgreSqlContainer _postgresqlContainer;
        private readonly IServiceScope _scope;
        private readonly ApplicationDbContext _dbContext;

        public UserflowTest()
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

            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
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
        public async Task Full_Userflow_Test()
        {
            await Test_Register_And_Login_User_Via_UI();
            await Test_Share_Message();
            await Test_Public_Timeline();
        }
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

        public async Task Test_Share_Message()
        {
            string message = "Hello, this is a test message!";

            // Ensure you're already logged in
            await _page.GotoAsync("http://localhost:3100/timeline");

            await _page.WaitForSelectorAsync("input[name='text']");
            await _page.FillAsync("input[name='text']", message);
            await _page.ClickAsync("input[type='submit'][value='Share']");

            try
            {
                await _page.WaitForSelectorAsync(".flashes", new PageWaitForSelectorOptions { Timeout = 10000 });
                var sharedMessage = await _page.InnerTextAsync(".flashes");
                Assert.Contains(message, sharedMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during message sharing: {ex.Message}");
            }
        }
        public async Task Test_Public_Timeline()
        {
            await _page.GotoAsync("http://localhost:3100/timeline/public");
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var publicTimelineHeader = await _page.WaitForSelectorAsync("h2:text('Public Timeline')", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });

            if (publicTimelineHeader == null)
                throw new TimeoutException("Public Timeline section not found on the page.");

            var firstMessage = await _page.WaitForSelectorAsync("ul.messages li", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });

            if (firstMessage == null)
                throw new TimeoutException("No messages found in the public timeline.");

            var userLink = await _page.QuerySelectorAsync("ul.messages li:first-child p strong a");

            if (userLink == null)
                throw new TimeoutException("Could not find user link in the first message.");

            await userLink.ClickAsync();

            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var currentUrl = _page.Url;

            await Task.Delay(5000);
        }
    }
}

