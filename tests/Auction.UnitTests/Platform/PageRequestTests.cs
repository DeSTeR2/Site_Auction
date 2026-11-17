// Page request normalizes out-of-range page and size instead of producing a negative offset.
using Auction.Core.Common;

namespace Auction.UnitTests.Platform;

public class PageRequestTests
{
    [Theory]
    [InlineData(0, 24, 1, 24)]
    [InlineData(-3, 24, 1, 24)]
    [InlineData(2, 0, 2, 1)]
    [InlineData(2, -5, 2, 1)]
    [InlineData(1, 1000, 1, PageRequest.MaxPageSize)]
    public void Out_of_range_values_are_normalized(int page, int pageSize, int expectedPage, int expectedSize)
    {
        var request = new PageRequest(page, pageSize);

        Assert.Equal(expectedPage, request.Page);
        Assert.Equal(expectedSize, request.PageSize);
        Assert.True(request.Skip >= 0);
    }

    [Fact]
    public void Skip_counts_items_on_previous_pages()
    {
        Assert.Equal(48, new PageRequest(3, 24).Skip);
    }
}
