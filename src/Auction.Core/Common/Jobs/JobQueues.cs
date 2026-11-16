// Background job queues of design/06-background-jobs.md: lot closing first, then notifications, then cleanup.
namespace Auction.Core.Common.Jobs;

public static class JobQueues
{
    public const string Critical = "critical";
    public const string Default = "default";
    public const string Maintenance = "maintenance";

    public static readonly string[] ByPriority = [Critical, Default, Maintenance];
}
