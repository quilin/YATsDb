namespace YATsDb.Client;

public sealed record Settings
{
    internal static string ClientName => "YATsDbHttpClient";

    public required string DbHost { get; init; }
}
