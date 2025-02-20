namespace minitwit.Domain.Entities;

public class Message
{
    public int Id { get; set; }
    public int AuthorId { get; set; }
    public virtual User Author { get; set; } = default!;
    public string Text { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public bool Flagged { get; set; } = false;
}