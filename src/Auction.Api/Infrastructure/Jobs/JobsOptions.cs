// Background job settings bound to the "Jobs" configuration section.
using System.ComponentModel.DataAnnotations;

namespace Auction.Api.Infrastructure.Jobs;

public sealed class JobsOptions
{
    public const string SectionName = "Jobs";
    public const string PostgreSql = "PostgreSql";
    public const string InMemory = "InMemory";

    [Required]
    public string Storage { get; init; } = PostgreSql;
}
