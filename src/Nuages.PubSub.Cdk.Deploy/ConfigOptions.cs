using System.Diagnostics.CodeAnalysis;

namespace Nuages.PubSub.Cdk.Deploy;

[ExcludeFromCodeCoverage]
public class ConfigOptions
{
    public string StackName { get; set; } = "";
    
    public WebSocket WebSocket { get; set; } = new ();
    public Api Api { get; set; } = new();
    
    public Env Env { get; set; } = new ();

    public DbProxy DatabaseDbProxy { get; set; } = new();
    public Data Data { get; set; } = new();
    
    public string? VpcId { get; set; }
    public string? SecurityGroup { get; set; }
}

[ExcludeFromCodeCoverage]
public class WebSocket
{
    public string? Domain { get; set; }
    public string? CertificateArn { get; set; } 
}

[ExcludeFromCodeCoverage]
public class Api
{
    public string? Domain { get; set; } 
    public string? CertificateArn { get; set; } 
    public string? ApiKey { get; set; } 
}

[ExcludeFromCodeCoverage]
public class Env
{
    public bool UseExternlAuth { get; set; }
    public Auth Auth { get; set; } = new();
    public ExternalAuth ExternalAuth { get; set; } = new();
}

[ExcludeFromCodeCoverage]
public class Auth
{
    public string? Audience { get; set; }
    public string? Issuer { get; set; }
    public string? Secret { get; set; }
}

[ExcludeFromCodeCoverage]
public class ExternalAuth
{
    public string? ValidIssuers { get; set; } 
    public string? ValidAudiences { get; set; }
    public string JsonWebKeySetUrlPath { get; set; } = ".well-known/openid-configuration";
    public bool DisableSslCheck { get; set; }
}

[ExcludeFromCodeCoverage]
public class Data
{
    public string? Storage { get; set; }
    
    public string? ConnectionString { get; set; }
}

[ExcludeFromCodeCoverage]
public class DbProxy
{
    public string? Arn { get; set; }
    public string? Name { get; set; }
    public string? Endpoint { get; set; }
    public  string? UserName { get; set; }
}
