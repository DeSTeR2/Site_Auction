// Single-level lot category with a fixed list filled by the migration (project.md §6.2).
namespace Auction.Core.Lots.Domain;

public sealed class Category
{
    private Category()
    {
        Name = string.Empty;
    }

    public Category(short id, string name, short sortOrder)
    {
        Id = id;
        Name = name;
        SortOrder = sortOrder;
    }

    public short Id { get; private set; }

    public string Name { get; private set; }

    public short SortOrder { get; private set; }
}
