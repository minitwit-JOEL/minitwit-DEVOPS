using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using minitwit.Application.Interfaces.Sim;
using minitwit.Infrastructure.Data;

namespace minitwit.Application.Services.Sim;

public class SimService : ISimService
{
    private readonly ApplicationDbContext _dbContext;

    public SimService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<int> GetLatestProcessedCommandId()
    {
        const string filePath = "../latest_processed_sim_action_id.txt";

        try
        {
            var content = await File.ReadAllTextAsync(filePath);

            content = content.Trim();

            if (int.TryParse(content, out var latestProcessedCommandId))
            {
                return latestProcessedCommandId;
            }
        }
        catch
        {
            return -1;
        }

        return -1;
    }

    public Task<IActionResult> UpdateLatest(HttpRequest request)
    {
        
        throw new NotImplementedException();
    }

    public bool CheckIfRequestFromSimulator(HttpRequest request)
    {
        const string expectedAuth = "Basic c2ltdWxhdG9yOnN1cGVyX3NhZmUh";
        return request.Headers.TryGetValue("Authorization", out var authHeader) && authHeader == expectedAuth;
    }
}