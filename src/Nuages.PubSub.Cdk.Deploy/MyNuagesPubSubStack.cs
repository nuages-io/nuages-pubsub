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
    private ISecurityGroup? _proxySg;
    private IDatabaseProxy? _proxy;

    // ReSharper disable once UnusedParameter.Local
    public MyNuagesPubSubStack(IConfiguration configuration, Construct scope, string id, IStackProps? props = null) 
        : base(scope, id, props)
    {
        WebSocketAsset = "./src/Nuages.PubSub.WebSocket.API/bin/Release/net6.0/linux-x64/publish";
        ApiAsset = "./src/Nuages.PubSub.API/bin/Release/net6.0/linux-x64/publish";

    }

    private IDatabaseProxy? Proxy
    {
        get
        {
            if (!string.IsNullOrEmpty(ProxyArn))
            {
                if (string.IsNullOrEmpty(ProxyName))
                    throw new Exception("ProxyName is required");

                if (string.IsNullOrEmpty(ProxyEndpoint))
                    throw new Exception("ProxyEndpoint is required");
                
                if (string.IsNullOrEmpty(ProxySecurityGroup))
                    throw new Exception("ProxySecurityGroup is required");
                
                _proxy ??= DatabaseProxy.FromDatabaseProxyAttributes(this, "Proxy", new DatabaseProxyAttributes
                {
                    DbProxyArn = ProxyArn,
                    DbProxyName = ProxyName,
                    Endpoint = ProxyEndpoint,
                    SecurityGroups = new[] { ProxySg }
                });
            }
           
            return _proxy;
        }
    }

    private ISecurityGroup ProxySg
    {
        get
        {
            if (_proxySg == null)
                _proxySg = SecurityGroup.FromLookupById(this, "WebApiSGDefault", "sg-88bf57e1");

            return _proxySg;
        }
    }

   
    

    protected override Function CreateWebSocketFunction(string name, string? handler, Role role, CfnApi api)
    {
        var func =  base.CreateWebSocketFunction(name, handler, role, api);

        Proxy?.GrantConnect(func, ProxyUser);
        
        return func;
    }

    protected override Function CreateWebApiFunction(string url, Role role)
    {
        var func = base.CreateWebApiFunction(url, role);

        Proxy?.GrantConnect(func,ProxyUser);
        
        return func;
    }
}