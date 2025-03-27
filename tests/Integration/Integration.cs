using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;
using minitwit.Infrastructure.Dtos.Requests;
using minitwit.Domain.Entities;
using minitwit.Infrastructure.Dtos.Sim;


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

        private async Task<string> GetLatest()
        {
            var response = await _client.GetAsync($"{BaseUrl}/latest");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(json);
            return result.latest.ToString();
        }

        private async Task RegisterUser(string username, string email, string pwd, int expectedLatest)
        {
            var registerRequest = new minitwit.Infrastructure.Dtos.Requests.RegisterRequest(username, email, pwd, pwd); // Use full namespace here
            var content = new StringContent(JsonConvert.SerializeObject(registerRequest), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync($"{BaseUrl}/register", content);
            response.EnsureSuccessStatusCode();

            var latest = await GetLatest();
            Assert.Equal(expectedLatest.ToString(), latest);
        }

        private async Task CreateMessage(string username, string messageContent, int expectedLatest)
        {
            var messageDto = new MessageDto
            {
                Content = messageContent,
                PubDate = DateTime.UtcNow,
                User = username
            };

            var content = new StringContent(JsonConvert.SerializeObject(messageDto), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync($"{BaseUrl}/msgs/{username}", content);
            response.EnsureSuccessStatusCode();

            var latest = await GetLatest();
            Assert.Equal(expectedLatest.ToString(), latest);
        }

        [Fact]
        public async Task Test_Register()
        {
            await RegisterUser("a", "a@a.a", "a", 1);
        }

        [Fact]
        public async Task Test_Create_Message()
        {
            await RegisterUser("a", "a@a.a", "a", 1);
            await CreateMessage("a", "Blub!", 2);
        }

        [Fact]
        public async Task Test_Get_Latest_User_Messages()
        {
            await RegisterUser("a", "a@a.a", "a", 1);
            await CreateMessage("a", "Blub!", 2);

            var query = new { no = 20, latest = 3 };
            var queryString = await new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("no", "20"), new KeyValuePair<string, string>("latest", "3") }).ReadAsStringAsync();
            var response = await _client.GetAsync($"{BaseUrl}/msgs/a?{queryString}");

            response.EnsureSuccessStatusCode();
            var messages = JsonConvert.DeserializeObject<List<MessageDto>>(await response.Content.ReadAsStringAsync());

            Assert.Contains(messages, m => m.Content == "Blub!" && m.User == "a");

            var latest = await GetLatest();
            Assert.Equal("3", latest);
        }

        [Fact]
        public async Task Test_Get_Latest_Messages()
        {
            await RegisterUser("a", "a@a.a", "a", 1);
            await CreateMessage("a", "Blub!", 2);

            var query = new { no = 20, latest = 4 };
            var queryString = await new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("no", "20"), new KeyValuePair<string, string>("latest", "4") }).ReadAsStringAsync();
            var response = await _client.GetAsync($"{BaseUrl}/msgs?{queryString}");

            response.EnsureSuccessStatusCode();
            var messages = JsonConvert.DeserializeObject<List<MessageDto>>(await response.Content.ReadAsStringAsync());

            Assert.Contains(messages, m => m.Content == "Blub!" && m.User == "a");

            var latest = await GetLatest();
            Assert.Equal("4", latest);
        }

        [Fact]
        public async Task Test_Follow_User()
        {
            await RegisterUser("a", "a@a.a", "a", 1);
            await RegisterUser("b", "b@b.b", "b", 2);

            var followRequest = new { Follow = "b" };
            var content = new StringContent(JsonConvert.SerializeObject(followRequest), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync($"{BaseUrl}/fllws/a", content);
            response.EnsureSuccessStatusCode();

            var query = new { no = 20, latest = 3 };
            var queryString = await new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("no", "20"), new KeyValuePair<string, string>("latest", "3") }).ReadAsStringAsync();
            var followResponse = await _client.GetAsync($"{BaseUrl}/fllws/a?{queryString}");
            followResponse.EnsureSuccessStatusCode();

            var follows = JsonConvert.DeserializeObject<dynamic>(await followResponse.Content.ReadAsStringAsync());
            var followsList = ((IEnumerable<dynamic>)follows.follows).Select(f => (string)f.ToString()).ToList();
            Assert.Contains("b", followsList);

            var latest = await GetLatest();
            Assert.Equal("3", latest);
        }

        [Fact]
        public async Task Test_Unfollow_User()
        {
            await RegisterUser("a", "a@a.a", "a", 1);
            await RegisterUser("b", "b@b.b", "b", 2);

            var followRequest = new { Follow = "b" };
            var content = new StringContent(JsonConvert.SerializeObject(followRequest), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync($"{BaseUrl}/fllws/a", content);
            response.EnsureSuccessStatusCode();

            // Now unfollow 'b'
            var unfollowRequest = new { Unfollow = "b" };
            content = new StringContent(JsonConvert.SerializeObject(unfollowRequest), Encoding.UTF8, "application/json");
            var unfollowResponse = await _client.PostAsync($"{BaseUrl}/fllws/a", content);
            unfollowResponse.EnsureSuccessStatusCode();

            var query = new { no = 20, latest = 3 };
            var queryString = await new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("no", "20"), new KeyValuePair<string, string>("latest", "3") }).ReadAsStringAsync();
            var followResponse = await _client.GetAsync($"{BaseUrl}/fllws/a?{queryString}");
            followResponse.EnsureSuccessStatusCode();

            var follows = JsonConvert.DeserializeObject<dynamic>(await followResponse.Content.ReadAsStringAsync());
            var followsList = ((IEnumerable<dynamic>)follows.follows).Select(f => (string)f.ToString()).ToList();
            Assert.DoesNotContain("b", followsList);

            // Verify the latest ID
            var latest = await GetLatest();
            Assert.Equal("4", latest);
        }
    }
}
