// Converts C# property paths ("Photos[0].Title") to the camelCase keys of the "errors" object.
using System.Text.Json;

namespace Auction.Api.Infrastructure.Validation;

internal static class FieldNames
{
    public static string ToKey(string propertyPath) =>
        string.Join('.', propertyPath.Split('.').Select(JsonNamingPolicy.CamelCase.ConvertName));
}
