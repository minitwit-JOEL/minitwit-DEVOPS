namespace minitwit.Infrastructure.Dtos.Sim
{
    public class MessageDto
    {
        public required string Content { get; set; }
        public DateTime PubDate { get; set; }
        public required string User { get; set; }
    }
}
