// Enum to snake_case text conversion used for every enum column.
using Auction.Core.Common.Enums;
using Auction.Infrastructure.Persistence;

namespace Auction.UnitTests.Infrastructure;

public class SnakeCaseEnumConverterTests
{
    [Fact]
    public void Multi_word_names_become_snake_case()
    {
        var converter = new SnakeCaseEnumConverter<ComplaintReason>();

        Assert.Equal("fraud_or_counterfeit", converter.ConvertToProvider(ComplaintReason.FraudOrCounterfeit));
        Assert.Equal("other", converter.ConvertToProvider(ComplaintReason.Other));
    }

    [Fact]
    public void Text_is_read_back_to_the_same_value()
    {
        var converter = new SnakeCaseEnumConverter<AdminActionType>();

        foreach (var action in Enum.GetValues<AdminActionType>())
        {
            var text = (string)converter.ConvertToProvider(action)!;

            Assert.Equal(action, converter.ConvertFromProvider(text));
        }
    }

    [Fact]
    public void Non_generic_factory_builds_a_converter_for_any_enum()
    {
        var converter = SnakeCaseEnumConverter.For(typeof(LotCondition));

        Assert.Equal("like_new", converter.ConvertToProvider(LotCondition.LikeNew));
    }
}
