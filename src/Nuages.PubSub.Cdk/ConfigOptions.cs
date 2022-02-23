using System.Diagnostics.CodeAnalysis;

namespace Nuages.PubSub.Cdk;

[ExcludeFromCodeCoverage]
public class ConfigOptions
{
    public string StackName { get; set; } = "";
    
    public WebSocket WebSocket { get; set; } = new ();
    public Api Api { get; set; } = new();
    
    public Env Env { get; set; } = new ();

    public Proxy Proxy { get; set; } = new();
    
    public string? VpcId { get; set; }
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
    public EnvData Data { get; set; } = new();
    public EnvPuSub PubSub { get; set; } = new();
}

[ExcludeFromCodeCoverage]
public class EnvData
{
    public bool? CreateDynamoDbTables { get; set; }
}

[ExcludeFromCodeCoverage]
public class EnvPuSub
{
    public string? Audience { get; set; }
    public string? Issuer { get; set; }
    public string? Secret { get; set; }
}

[ExcludeFromCodeCoverage]
public class Proxy
{
    public string? Arn { get; set; }
    public string? Name { get; set; }
    public string? SecurityGroup { get; set; }
    public string? Endpoint { get; set; }
    public  string? UserName { get; set; }
}
