namespace SharedKernel.Models;

public class PaginatedResult<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PaginatedResult()
    {
    }

    public PaginatedResult(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public static PaginatedResult<T> Create(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
    {
        return new PaginatedResult<T>(items, totalCount, pageNumber, pageSize);
    }

    public static PaginatedResult<T> Empty(int pageNumber = 1, int pageSize = 10)
    {
        return new PaginatedResult<T>(Enumerable.Empty<T>(), 0, pageNumber, pageSize);
    }
}