using System.Diagnostics.CodeAnalysis;
using Amazon.CDK;
using Amazon.CDK.AWS.Apigatewayv2;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.RDS;
using Constructs;
using Microsoft.Extensions.Configuration;
using Nuages.PubSub.WebSocket.API;

namespace Nuages.PubSub.Cdk.Deploy;

[ExcludeFromCodeCoverage]
public class MyNuagesPubSubStack : NuagesPubSubWebSocketCdkStack<PubSubFunction>
{
    private ISecurityGroup? _sg;
    private IDatabaseProxy? _proxy;

    // ReSharper disable once UnusedParameter.Local
    public MyNuagesPubSubStack(IConfiguration configuration, Construct scope, string id, IStackProps? props = null) 
        : base(scope, id, props)
    {
        WebSocketAsset = "./src/Nuages.PubSub.WebSocket.API/bin/Release/net6.0/linux-x64/publish";
        ApiAsset = "./src/Nuages.PubSub.API/bin/Release/net6.0/linux-x64/publish";

        ProxyArn = configuration["Env:Data:ProxyArn"];
        
        
       
    }

    IDatabaseProxy Proxy
    {
        get
        {
            if (_proxy == null)
            {
                _proxy = DatabaseProxy.FromDatabaseProxyAttributes(this, "Proxy", new DatabaseProxyAttributes
                {
                    DbProxyArn = ProxyArn,
                    DbProxyName = "mysql-dev",
                    Endpoint = "mysql-dev.proxy-cprjwrwlrdac.ca-central-1.rds.amazonaws.com",
                    SecurityGroups = new [] { SG}
                });
            }

            return _proxy;
        }
    }
    
    ISecurityGroup SG
    {
        get
        {
            if (_sg == null)
                _sg = SecurityGroup.FromLookupById(this, "WebApiSGDefault", "sg-88bf57e1");

            return _sg;
        }
    }

    public string ProxyArn { get; set; }

    protected override Function CreateWebSocketFunction(string name, string? handler, Role role, CfnApi api)
    {
        var func =  base.CreateWebSocketFunction(name, handler, role, api);

        if (!string.IsNullOrEmpty(ProxyArn))
        {
            Proxy.GrantConnect(func,"admin");
        }
        
        
        return func;
    }

    protected override Function CreateWebApiFunction(string url, Role role)
    {
        var func = base.CreateWebApiFunction(url, role);

        if (!string.IsNullOrEmpty(ProxyArn))
        {

            Proxy.GrantConnect(func,"admin");
        }
        
        return func;
    }
}