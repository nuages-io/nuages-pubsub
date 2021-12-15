using System.Diagnostics.CodeAnalysis;

namespace Nuages.PubSub.Samples.WebSocket.Deploy;

[ExcludeFromCodeCoverage]
public class ConfigOptions
{
    public string StackName { get; set; } = "";
    public string TableNamePrefix { get; set; }= "";
    public bool CreateDynamoDbStorage { get; set; } = true;
    
    public WebSocket WebSocket { get; set; } = new ();
    public Api Api { get; set; } = new();
}

[ExcludeFromCodeCoverage]
public class WebSocket
{
    public string Domain { get; set; } = "";
    public string CertificateArn { get; set; } = "";
}

[ExcludeFromCodeCoverage]
public class Api
{
    public string Domain { get; set; } = "";
    public string CertificateArn { get; set; } = "";
    public string ApiKey { get; set; } = "";
}
