namespace AspNetCore.Settings;

public sealed class OpenRouterSettings
{
    public static readonly string SectionName = "OpenRouter";
    public Uri BaseUrl { get; init; } = null!;
    public Uri? ProxyUrl { get; init; }
    public string ApiKey { get; init; } = null!;
    public string Model { get; init; } = null!;
}