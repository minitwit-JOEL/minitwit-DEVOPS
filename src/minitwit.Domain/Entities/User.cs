namespace minitwit.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required string PasswordHash { get; init; }
    
    public required string Salt { get; init; }
    
    public virtual ICollection<User> Follows { get; set; } = new List<User>();
}