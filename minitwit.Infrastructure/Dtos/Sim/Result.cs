namespace minitwit.Infrastructure.Dtos.Sim;

public class Result
{
    public bool IsSuccess { get; private set; }
    public string ErrorMessage { get; private set; }

    public static Result Success() => new Result { IsSuccess = true };
    public static Result Failure(string errorMessage) =>
        new Result { IsSuccess = false, ErrorMessage = errorMessage };
}