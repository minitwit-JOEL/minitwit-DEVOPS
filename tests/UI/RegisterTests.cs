using Microsoft.Playwright;
using System.Threading.Tasks;
using Xunit;

namespace UI.Tests
{
    public class RegisterTests : IAsyncLifetime
    {
        private IPlaywright _playwright;
        private IBrowser _browser;
        private IPage _page;

        public RegisterTests()
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
        public async Task Test_Register_User_Via_UI()
        {
            await _page.GotoAsync("http://localhost:3100/register");

            await _page.WaitForSelectorAsync("input[name='username']");

            await _page.FillAsync("input[name='username']", "Me");
            await _page.FillAsync("input[name='email']", "me@some.where");
            await _page.FillAsync("input[name='password']", "secure123");
            await _page.FillAsync("input[name='passwordRepeat']", "secure123");

            await _page.ClickAsync("input[type='submit'][value='Sign Up']");

            var successMessage = await _page.InnerTextAsync(".flashes");

            var expectedMessage = "You were successfully registered and can login now";
            Assert.Equal(expectedMessage, successMessage);
        }
    }
}
