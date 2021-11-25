namespace Nuages.PubSub.WebSocket.Model;

public class PubSubAuthOptions
{
    public string ValidIssuers { get; set; } = "";
    public string? ValidAudiences { get; set; }
    public string? Secret { get; set; }
    public string JsonWebKeySetUrlPath { get; set; } = ".well-known/jwks.json";
    public bool DisableSslCheck { get; set; }
    public string? Issuer { get; set; }
}