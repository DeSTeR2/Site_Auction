// Maps the analytics_events table with a jsonb payload and a (type, created_at) index.
using Auction.Core.Platform.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auction.Infrastructure.Persistence.Platform;

public sealed class AnalyticsRecordConfiguration : IEntityTypeConfiguration<AnalyticsRecord>
{
    public void Configure(EntityTypeBuilder<AnalyticsRecord> builder)
    {
        builder.ToTable("analytics_events");

        builder.HasKey(record => record.Id);

        builder.Property(record => record.Type).IsRequired();
        builder.Property(record => record.Payload).HasColumnType("jsonb").IsRequired();

        builder.HasIndex(record => new { record.Type, record.CreatedAt });
    }
}
