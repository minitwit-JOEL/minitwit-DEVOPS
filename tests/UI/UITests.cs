using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using Xunit;
using Microsoft.EntityFrameworkCore;
using minitwit.Infrastructure.Data;
using minitwit.Domain.Entities;
using System;
using System.Linq;

namespace UI.Tests
{
    public class RegisterTests : IDisposable
    {
        private const string GUI_URL = "http://localhost:5000/register";
        private readonly ApplicationDbContext _dbContext;

        public RegisterTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql("Host=localhost;Port=5432;Database=test_minitwit;Username=test_user;Password=test_password") // Change
                .Options;

            _dbContext = new ApplicationDbContext(options);
        }

        private IWebDriver InitializeDriver()
        {
            var options = new ChromeOptions();
            // Uncomment the line below to run in headless mode
            // options.AddArgument("--headless");

            return new ChromeDriver(options);
        }

        private void RegisterUserViaGui(IWebDriver driver, string[] data)
        {
            driver.Navigate().GoToUrl(GUI_URL);

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            var inputFields = driver.FindElements(By.TagName("input"));

            for (int idx = 0; idx < data.Length; idx++)
            {
                inputFields[idx].SendKeys(data[idx]);
            }

            inputFields[4].SendKeys(Keys.Return);

            wait.Until(d => d.FindElements(By.ClassName("flashes")).Count > 0);
        }

        [Fact]
        public void Test_Register_User_Via_GUI()
        {
            using (var driver = InitializeDriver())
            {
                RegisterUserViaGui(driver, new[] { "Me", "me@some.where", "secure123", "secure123" });

                var successMessage = driver.FindElement(By.ClassName("flashes")).Text;
                var expectedMessage = "You were successfully registered and can login now";
                Assert.Equal(expectedMessage, successMessage);
            }

            CleanUpDatabase("Me");
        }

        private void CleanUpDatabase(string username)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Username == username);

            if (user != null)
            {
                _dbContext.Users.Remove(user);
                _dbContext.SaveChanges();
            }
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}
