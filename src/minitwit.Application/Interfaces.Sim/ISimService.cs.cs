using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace minitwit.Application.Interfaces.Sim;

public interface ISimService {
    public Task<int> GetLatestProcessedCommandId();
    public Task UpdateLatest(int newProcessedId);
    public bool CheckIfRequestFromSimulator(HttpRequest request);
}