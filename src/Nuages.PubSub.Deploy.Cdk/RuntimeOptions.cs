namespace Nuages.PubSub.Deploy.Cdk;

public class RuntimeOptions
{
    public ExternalAuth ExternalAuth { get; set; } = new();
    public Auth Auth { get; set; } = new();
    public Data Data { get; set; } = new();
}

public class Data
{
    public string? Storage { get; set; }
    public string? ConnectionString { get; set; }
}

public class ExternalAuth
{
    public string ValidIssuers { get; set; } = "";
    public string? ValidAudiences { get; set; }
    public string Roles { get; set; } = "SendMessageToGroup JoinOrLeaveGroup";
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