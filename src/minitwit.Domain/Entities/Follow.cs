using System.Text.Json.Serialization;

namespace minitwit.Domain.Entities;

public class Follow
{
    public int Id { get; set; }
    
    public int WhoId { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public virtual User Who { get; set; }
    
    public int WhomId { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public virtual User Whom { get; set; }
}