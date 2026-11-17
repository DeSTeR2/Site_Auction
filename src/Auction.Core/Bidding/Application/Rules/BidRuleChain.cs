// Runs the bid rules of one phase in a fixed order and stops at the first rejection.
using Auction.Core.Bidding.Domain;
using Auction.Core.Common;

namespace Auction.Core.Bidding.Application.Rules;

public sealed class BidRuleChain
{
    private readonly Dictionary<BidPhase, IBidRule[]> rulesByPhase;

    public BidRuleChain(IEnumerable<IBidRule> rules)
    {
        rulesByPhase = rules
            .GroupBy(r => r.Phase)
            .ToDictionary(g => g.Key, g => g.OrderBy(r => r.Order).ToArray());
    }

    public BidRuleResult Run(BidPhase phase, BidContext context)
    {
        if (!rulesByPhase.TryGetValue(phase, out var phaseRules))
        {
            return BidRuleResult.Pass;
        }

        foreach (var rule in phaseRules)
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
