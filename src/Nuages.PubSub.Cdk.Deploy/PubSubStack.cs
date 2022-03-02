using System.Diagnostics.CodeAnalysis;
using Amazon.CDK;
using Constructs;
using Nuages.PubSub.WebSocket.API;

namespace Nuages.PubSub.Cdk.Deploy;

[ExcludeFromCodeCoverage]
public class PubSubStack : PubSubWebSocketCdkStack<PubSubFunction>
{
    public static void CreateStack(Construct scope, ConfigOptions options)
    {
        var stack = new PubSubStack(scope, options.StackName, new StackProps
        {
            StackName = options.StackName,
            Env = new Amazon.CDK.Environment
            {
                Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION")
            }
        })
        {
            WebSocketDomainName = options.WebSocket.Domain,
            WebSocketCertificateArn = options.WebSocket.CertificateArn,
            ApiDomainName = options.Api.Domain,
            ApiCertificateArn = options.Api.CertificateArn,
            ApiApiKey = options.Api.ApiKey,
            VpcId = options.VpcId,
            SecurityGroupId = options.SecurityGroupId,
            DatabaseProxyArn = options.DatabaseDbProxy.Arn,
            DatabaseProxyEndpoint = options.DatabaseDbProxy.Endpoint,
            DatabaseProxyName = options.DatabaseDbProxy.Name,
            DatabaseProxyUser = options.DatabaseDbProxy.UserName
        };

        //stack.InitializeFromOptions(options);
        
        stack.BuildStack();
    }
    
    // ReSharper disable once UnusedParameter.Local
    private PubSubStack(Construct scope, string id, IStackProps? props = null) 
        : base(scope, id, props)
    {
        WebSocketAsset = "./src/Nuages.PubSub.WebSocket.API/bin/Release/net6.0/linux-x64/publish";
        ApiAsset = "./src/Nuages.PubSub.API/bin/Release/net6.0/linux-x64/publish";
    }
    
    // private void InitializeFromOptions(ConfigOptions options)
    // {
    //     // if (!string.IsNullOrEmpty(options.WebSocket.Domain))
    //     //     Node.SetContext(ContextValues.WebSocketDomain,  options.WebSocket.Domain);
    //
    //     WebSocketDomainName = options.WebSocket.Domain;
    //     
    //     // if (!string.IsNullOrEmpty(options.WebSocket.CertificateArn))
    //     //     Node.SetContext(ContextValues.WebSocketCertificateArn,options.WebSocket.CertificateArn);
    //
    //     WebSocketCertificateArn = options.WebSocket.CertificateArn;
    //     
    //     // if (!string.IsNullOrEmpty(options.Api.Domain))
    //     //     Node.SetContext(ContextValues.ApiDomain,  options.Api.Domain);
    //
    //     ApiDomainName = options.Api.Domain;
    //     
    //     // if (!string.IsNullOrEmpty(options.Api.CertificateArn))
    //     //     Node.SetContext(ContextValues.ApiCertificateArn, options.Api.CertificateArn);
    //
    //     ApiCertificateArn = options.Api.CertificateArn;
    //     
    //     // if (!string.IsNullOrEmpty(options.Api.ApiKey))
    //     //     Node.SetContext(ContextValues.ApiApiKey, options.Api.ApiKey);
    //
    //     ApiApiKey = options.Api.ApiKey;
    //     
    //     // if (!string.IsNullOrEmpty( options.VpcId))
    //     //     Node.SetContext(ContextValues.VpcId, options.VpcId);
    //
    //     VpcId = VpcId;
    //     
    //     // if (!string.IsNullOrEmpty( options.SecurityGroup))
    //     //     Node.SetContext(ContextValues.SecurityGroupId, options.SecurityGroup);
    //
    //     SecurityGroupId = options.SecurityGroupId;
    //     
    //     // if (!string.IsNullOrEmpty( options.DatabaseDbProxy.Arn))
    //     //     Node.SetContext(ContextValues.DatabaseProxyArn, options.DatabaseDbProxy.Arn);
    //
    //     DatabaseProxyArn = options.DatabaseDbProxy.Arn;
    //     //
    //     // if (!string.IsNullOrEmpty( options.DatabaseDbProxy.Endpoint))
    //     //     Node.SetContext(ContextValues.DatabaseProxyEndpoint, options.DatabaseDbProxy.Endpoint);
    //
    //     DatabaseProxyEndpoint = options.DatabaseDbProxy.Endpoint;
    //     
    //     // if (!string.IsNullOrEmpty( options.DatabaseDbProxy.Name))
    //     //     Node.SetContext(ContextValues.DatabaseProxyName, options.DatabaseDbProxy.Name);
    //
    //     DatabaseProxyName = options.DatabaseDbProxy.Name;
    //     
    //     // if (!string.IsNullOrEmpty( options.DatabaseDbProxy.UserName))
    //     //     Node.SetContext(ContextValues.DatabaseProxyUser, options.DatabaseDbProxy.UserName);
    //
    //     DatabaseProxyUser = options.DatabaseDbProxy.UserName;
    //
    // }
}