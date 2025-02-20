using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using minitwit.Application.Interfaces.Sim;
using minitwit.Domain.Entities;
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
        var lastProcessedId = await _dbContext.ProcessedActions.OrderByDescending(p => p.Id).FirstOrDefaultAsync();
        
        return lastProcessedId?.ProcessId ?? -1;
    }

    public async Task UpdateLatest(int newProcessedId)
    {
        await _dbContext.ProcessedActions.AddAsync(new ProcessedAction
        {
            ProcessId = newProcessedId
        });
        await _dbContext.SaveChangesAsync();
    }

    public bool CheckIfRequestFromSimulator(HttpRequest request)
    {
        const string expectedAuth = "Basic c2ltdWxhdG9yOnN1cGVyX3NhZmUh";
        return request.Headers.TryGetValue("Authorization", out var authHeader) && authHeader == expectedAuth;
    }
}