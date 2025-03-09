
using System.Dynamic;
using minitwit.Domain.Entities;

public class PaginationData
{
    public int PageSize = 50;

    public int Total { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
}

public class PaginationResponse
{
    public IEnumerable<Message>? Data { get; set; }
    public PaginationData? Pagination { get; set; }
}

