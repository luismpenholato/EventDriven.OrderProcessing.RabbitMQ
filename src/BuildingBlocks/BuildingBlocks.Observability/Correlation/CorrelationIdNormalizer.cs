namespace BuildingBlocks.Observability.Correlation;

internal static class CorrelationIdNormalizer
{
    public const string HeaderName = "X-Correlation-Id";

    public static string Generate() => Guid.NewGuid().ToString("N");

    public static string FromGuid(Guid correlationId) => correlationId.ToString("N");

    public static Guid ToGuid(string correlationId)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
            throw new ArgumentException("Correlation id is required.", nameof(correlationId));

        var normalized = correlationId.Trim();
        return normalized.Length == 32
            ? Guid.ParseExact(normalized, "N")
            : Guid.Parse(normalized);
    }
}
