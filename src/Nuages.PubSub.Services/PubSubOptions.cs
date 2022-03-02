using System.Diagnostics.CodeAnalysis;

namespace Nuages.PubSub.Services;

[ExcludeFromCodeCoverage]
public class PubSubOptions
{
    /// <summary>
    /// Uri of the PubSub WebSocket API Gateway
    /// </summary>
    /// 
    public string Uri { get; set; } = "wss://name=will-be-set-at-deploy";
    
    /// <summary>
    /// Region where the stack is deployed
    /// </summary>
    public string Region { get; set; } = "region-set-at-deplopy";
    
    /// <summary>
    /// Name of the stack
    /// </summary>
    public string StackName { get; set; } = "name-set-at-deploy";

    public Auth Auth { get; set; } = new ();
    public ExternalAuth ExternalAuth { get; set; } = new ();
}

[ExcludeFromCodeCoverage]
public class ExternalAuth
{
    public string ValidIssuers { get; set; } = "";
    public string? ValidAudiences { get; set; }
    public string JsonWebKeySetUrlPath { get; set; } = ".well-known/jwks";
    public bool DisableSslCheck { get; set; }
}

public class Auth
{
    /// <summary>
    /// Issuer name of the JWT token. It can be any value. Values configured in API and WebSocket endpoint must match.
    /// Can be set at deployment or runtime
    /// </summary>
    public string Issuer { get; set; } = string.Empty;
    
    /// <summary>
    /// Audience name of the JWT token. It can be any value. Values configured in API and WebSocket endpoint must match.
    /// Can be set at deployment or runtime
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// JWT Secret for token validation. It is recommender to keep in a secure place.
    /// Can be set at deployment or runtime
    /// </summary>
    public string Secret { get; set; } = string.Empty;


}