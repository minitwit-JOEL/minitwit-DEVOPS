using Microsoft.Playwright;
using System.Threading.Tasks;
using Xunit;

namespace UI.Tests
{
    public class RegisterAndLoginTests : IAsyncLifetime
    {
        private IPlaywright _playwright;
        private IBrowser _browser;
        private IPage _page;

        public RegisterAndLoginTests()
        {
            _playwright = Playwright.CreateAsync().Result;
        }

        public async Task InitializeAsync()
        {
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
            _page = await _browser.NewPageAsync();
        }

        public async Task DisposeAsync()
        {
            if (_browser != null)
            {
                await _browser.CloseAsync();
            }
        }

        [Fact]
        public async Task Test_Register_And_Login_User_Via_UI()
        {
            await _page.GotoAsync("http://localhost:3100/register");

            await _page.WaitForSelectorAsync("input[name='username']");

            string username = "Me";
            string email = "me@some.where";
            string password = "secure123";

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
