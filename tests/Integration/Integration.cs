using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;

namespace minitwit.Tests.Integration
{
    public class IntegrationTests
    {
        private readonly HttpClient _client;
        private const string BaseUrl = "http://localhost:5034";

        public IntegrationTests()
        {
            _client = new HttpClient();
        }

        private async Task RegisterUser(string username, string email, string password)
        {
            var registerRequest = new
            {
                Username = username,
                Email = email,
                Password = password,
                ConfirmPassword = password
            };

            var content = new StringContent(JsonConvert.SerializeObject(registerRequest), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync($"{BaseUrl}/api/auth/register", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error in registration: {errorContent}");
            }

            response.EnsureSuccessStatusCode();
        }

        private async Task LoginUser(string username, string password)
        {
            var loginRequest = new
            {
                Username = username,
                Password = password
            };

            var content = new StringContent(JsonConvert.SerializeObject(loginRequest), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync($"{BaseUrl}/api/auth/login", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error in login: {errorContent}");
            }

            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.Contains("token", responseBody);
        }

        [Fact]
        public async Task Test_Register()
        {
            await RegisterUser("abc123", "abc123@example.com", "password123");
        }

        [Fact]
        public async Task Test_Login()
        {
            await LoginUser("abc123", "password123");
        }
    }
}
