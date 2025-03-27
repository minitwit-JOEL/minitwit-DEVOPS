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
        private const string BaseUrl = "http://localhost:3100";

        public IntegrationTests()
        {
            _client = new HttpClient();
        }

        // Register a user
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
            var response = await _client.PostAsync($"{BaseUrl}/register", content);

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
            var response = await _client.PostAsync($"{BaseUrl}/login", content);

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
            await RegisterUser("ttttestuser", "testuser@example.com", "password123");
        }

        [Fact]
        public async Task Test_Login()
        {
            // should perhaps be deleted if Test_Register is run first, but just in case
            await RegisterUser("estuser", "testuser@example.com", "password123");

            await LoginUser("estuser", "password123");
        }
    }
}
