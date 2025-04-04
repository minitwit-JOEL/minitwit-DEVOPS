using System.Text.Json.Serialization;

namespace minitwit.Domain.Entities;

public class Follow
{
    public int Id { get; init; }
    
    public int WhoId { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public virtual User Who { get; init; } = null!;
    
    public int WhomId { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public virtual User Whom { get; init; } = null!;
}