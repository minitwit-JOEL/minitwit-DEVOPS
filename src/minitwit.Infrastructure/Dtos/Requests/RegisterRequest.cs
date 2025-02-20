namespace minitwit.Infrastructure.Dtos.Requests;

public record RegisterRequest(string Username, string Email, string Password, string ConfirmPassword);