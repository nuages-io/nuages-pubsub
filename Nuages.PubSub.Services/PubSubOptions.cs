using System.Diagnostics.CodeAnalysis;

namespace Nuages.PubSub.Services;

[ExcludeFromCodeCoverage]
public class PubSubOptions
{
    public string ValidIssuers { get; set; } = "";
    public string? ValidAudiences { get; set; }
    public string? Secret { get; set; }
    public string JsonWebKeySetUrlPath { get; set; } = ".well-known/jwks.json";
    public bool DisableSslCheck { get; set; }
    public string? Issuer { get; set; }
    public string? Uri { get; set; }
    public string Region { get; set; } = "";
    public string? TableNamePrefix { get; set; }
}