using System.Diagnostics.CodeAnalysis;

namespace Nuages.PubSub.Services;

[ExcludeFromCodeCoverage]
public class PubSubOptions
{
    public string? Secret { get; set; }
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public string? Uri { get; set; }
    public string Region { get; set; } = "";
    public string StackName { get; set; } = "";
}

[ExcludeFromCodeCoverage]
public class PubSubExternalAuthOption
{
    public string ValidIssuers { get; set; } = "";
    public string? ValidAudiences { get; set; }
    public string JsonWebKeySetUrlPath { get; set; } = ".well-known/openid-configuration";
    public bool DisableSslCheck { get; set; }
}