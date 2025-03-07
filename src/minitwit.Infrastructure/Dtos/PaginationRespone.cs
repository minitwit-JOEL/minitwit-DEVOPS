
using System.Dynamic;
using minitwit.Domain.Entities;

public class PaginationResponse
{
    public int PageSize = 50;

    public int Total { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
}

public class Temp
{
    public Message[] Data { get; set; }
    public PaginationResponse Pagination { get; set; }
}

