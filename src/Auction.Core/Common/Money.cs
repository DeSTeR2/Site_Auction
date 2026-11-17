// Formats whole-hryvnia amounts the way the UI and error texts show them.
using System.Globalization;

namespace Auction.Core.Common;

public static class Money
{
    private static readonly NumberFormatInfo GroupedBySpace = new()
    {
        NumberGroupSeparator = " ",
        NumberGroupSizes = [3]
    };

    public static string Format(long amount) => $"{amount.ToString("#,0", GroupedBySpace)} грн";
}
