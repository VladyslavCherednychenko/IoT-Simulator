namespace SimulatorApp.Core.Extensions;

public static class StringExtensions
{
    public static string ToSlugString(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        return input
            .Trim()
            .Replace(" ", "_")
            .Replace("/", "-") // To prevent accidental sub-topic creation
            .ToLowerInvariant();
    }
}
