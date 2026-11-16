// Maps categories and seeds the fixed list of 11 categories from project.md §6.2.
using Auction.Core.Lots.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auction.Infrastructure.Persistence.Lots;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");

        builder.HasKey(category => category.Id);
        builder.Property(category => category.Id).ValueGeneratedNever();
        builder.Property(category => category.Name).IsRequired();

        builder.HasIndex(category => category.Name).IsUnique();

        builder.HasData(
            new Category(1, "Монети і банкноти", 1),
            new Category(2, "Марки", 2),
            new Category(3, "Вініл і музика", 3),
            new Category(4, "Книги", 4),
            new Category(5, "Електроніка і техніка", 5),
            new Category(6, "Одяг і аксесуари", 6),
            new Category(7, "Антикваріат", 7),
            new Category(8, "Мистецтво і хендмейд", 8),
            new Category(9, "Іграшки та колекційні фігурки", 9),
            new Category(10, "Спорт і хобі", 10),
            new Category(11, "Інше", 11));
    }
}
