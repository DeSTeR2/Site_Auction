// Page request and page response shared by list endpoints.
namespace Auction.Core.Common;

public sealed record PageRequest
{
    public const int MaxPageSize = 100;

    public PageRequest(int page, int pageSize)
    {
        Page = Math.Max(page, 1);
        PageSize = Math.Clamp(pageSize, 1, MaxPageSize);
    }

    public int Page { get; }

    public int PageSize { get; }

    public int Skip => (Page - 1) * PageSize;
}

public sealed record PagedList<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
