namespace minitwit.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public virtual ICollection<User> Follows { get; set; } = new List<User>();
}