namespace Nuages.PubSub.Cdk;

// ReSharper disable once InconsistentNaming
public static class ContextValues
{
    public const string WebSocketDomain = "WebSocketDomain";
    public const string WebSocketCertificateArn = "WebSocketCertificateArn";

    public const string ApiDomain = "ApiDomain";
    public const string ApiCertificateArn = "ApiCertificateArn";
    public const string ApiApiKey = "ApiApiKey";

    public const string AuthAudience = "AuthAudience";
    public const string AuthIssuer = "AuthIssuer";
    public const string AuthSecret = "AuthSecret";

    public const string VpcId = "VpcId";
    public const string SecurityGroupId = "SecurityGroupId";

    public const string DatabaseProxyArn = "DatabaseProxyArn";
    public const string DatabaseProxyName = "DatabaseProxyName";
    public const string DatabaseProxyEndpoint = "DatabaseProxyEndpoint";
    public const string DatabaseProxyUser = "DatabaseProxyUser";

    public const string DataStorage = "DataStorage";
    public const string DataConnectionString = "DataConnectionString";
    
    public const string ExternalAuthValidAudiences = "ExternalAuthValidAudiences";
    public const string ExternalAuthValidIssuers = "ExternalAuthValidIssuers";
    public const string ExternalAuthJsonWebKeySetUrlPath = "ExternalAuthJsonWebKeySetUrlPath";
    public const string ExternalAuthDisableSslCheck = "ExternalAuthDisableSslCheck";
}