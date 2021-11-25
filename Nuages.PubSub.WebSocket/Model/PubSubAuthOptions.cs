namespace Nuages.PubSub.WebSocket.Model;

public class PubSubAuthOptions
{
    public string Issuers { get; set; } = "";
    public string? Audiences { get; set; }
    public string? Secret { get; set; }
    public string JsonWebKeySetUrlPath { get; set; } = ".well-known/jwks.json";
    public bool DisableSslCheck { get; set; }
}