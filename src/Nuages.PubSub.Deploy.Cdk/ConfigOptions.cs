using System.Diagnostics.CodeAnalysis;

namespace Nuages.PubSub.Deploy.Cdk;

[ExcludeFromCodeCoverage]
public class ConfigOptions
{
    public string StackName { get; set; } = "";
    
    public WebSocket WebSocket { get; set; } = new ();
    public Api Api { get; set; } = new();
    
    public DbProxy DatabaseDbProxy { get; set; } = new();
    
    public string? VpcId { get; set; }
    public string? SecurityGroupId { get; set; }

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
public class DbProxy
{
    public string? Arn { get; set; }
    public string? Name { get; set; }
    public string? Endpoint { get; set; }
    public  string? UserName { get; set; }
}

