// Runs the bid rules of one phase in a fixed order and stops at the first rejection.
using Auction.Core.Bidding.Domain;
using Auction.Core.Common;

namespace Auction.Core.Bidding.Application.Rules;

public sealed class BidRuleChain(IEnumerable<IBidRule> rules)
{
    public BidRuleResult Run(BidPhase phase, BidContext context)
    {
        foreach (var rule in rules.Where(r => r.Phase == phase).OrderBy(r => r.Order))
        {
            var result = rule.Check(context);
            if (!result.IsPass)
            {
                return result;
            }
        }

        return BidRuleResult.Pass;
    }

    public Result<Unit> Check(BidPhase phase, BidContext context)
    {
        var result = Run(phase, context);
        return result.IsPass
            ? Result.Success()
            : BidRejectionFactory.Create(result.Reason!.Value, result.Data);
    }
}
