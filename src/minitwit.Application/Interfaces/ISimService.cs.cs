namespace minitwit.Application.Interfaces;

public interface ISimService {
    public void UpdateLatest(HttpRequestError request);
    public string CheckIfRequestFromSimulator(HttpRequestError request);
}