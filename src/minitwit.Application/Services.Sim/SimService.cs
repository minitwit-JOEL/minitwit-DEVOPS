using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using minitwit.Application.Interfaces.Sim;
using minitwit.Domain.Entities;
using minitwit.Infrastructure.Data;
using minitwit.Infrastructure.Dtos.Sim;

namespace minitwit.Application.Services.Sim;

public class SimService : ISimService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly SimApiAccess _SimApiAccess;

    public SimService(ApplicationDbContext dbContext, IOptions<SimApiAccess> options)
    {
        _dbContext = dbContext;
        _SimApiAccess = options.Value;
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
        string expectedAuth =_SimApiAccess.Key is null ? "" : _SimApiAccess.Key;
        return request.Headers.TryGetValue("Authorization", out var authHeader) && authHeader == expectedAuth;
    }
}