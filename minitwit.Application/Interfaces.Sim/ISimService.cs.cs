using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace minitwit.Application.Interfaces.Sim;

public interface ISimService {
    public Task<int> GetLatestProcessedCommandId();
    public Task<IActionResult> UpdateLatest(HttpRequest request);
    public bool CheckIfRequestFromSimulator(HttpRequest request);
}