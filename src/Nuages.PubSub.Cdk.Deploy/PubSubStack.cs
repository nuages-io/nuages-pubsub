using System.Diagnostics.CodeAnalysis;
using Amazon.CDK;
using Constructs;
using Microsoft.Extensions.Configuration;
using Nuages.PubSub.WebSocket.API;

namespace Nuages.PubSub.Cdk.Deploy;

[ExcludeFromCodeCoverage]
public class PubSubStack : PubSubWebSocketCdkStack<PubSubFunction>
{
    
    public static void CreateStack(Construct scope, IConfiguration configuration)
    {
        var options = configuration.Get<ConfigOptions>();
        
        var stack = new PubSubStack(scope, options.StackName, new StackProps
        {
            StackName = options.StackName,
            Env = new Amazon.CDK.Environment
            {
                Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION")
            }
        });

        stack.InitializeContextFromOptions(options);
        
        stack.CreateTemplate();
    }
    
    // ReSharper disable once UnusedParameter.Local
    private PubSubStack(Construct scope, string id, IStackProps? props = null) 
        : base(scope, id, props)
    {
        WebSocketAsset = "./src/Nuages.PubSub.WebSocket.API/bin/Release/net6.0/linux-x64/publish";
        ApiAsset = "./src/Nuages.PubSub.API/bin/Release/net6.0/linux-x64/publish";
    }
    
    private void InitializeContextFromOptions(ConfigOptions options)
    {
        if (!string.IsNullOrEmpty(options.WebSocket.Domain))
            Node.SetContext(ContextValues.WebSocketDomain,  options.WebSocket.Domain);
        
        if (!string.IsNullOrEmpty(options.WebSocket.CertificateArn))
            Node.SetContext(ContextValues.WebSocketCertificateArn,options.WebSocket.CertificateArn);
        
        if (!string.IsNullOrEmpty(options.Api.Domain))
            Node.SetContext(ContextValues.ApiDomain,  options.Api.Domain);
        
        if (!string.IsNullOrEmpty(options.Api.CertificateArn))
            Node.SetContext(ContextValues.ApiCertificateArn, options.Api.CertificateArn);
        
        if (!string.IsNullOrEmpty(options.Api.ApiKey))
            Node.SetContext(ContextValues.ApiApiKey, options.Api.ApiKey);
        
        if (!string.IsNullOrEmpty( options.VpcId))
            Node.SetContext(ContextValues.VpcId, options.VpcId);
        
        if (!string.IsNullOrEmpty( options.SecurityGroup))
            Node.SetContext(ContextValues.SecurityGroupId, options.SecurityGroup);
        
        if (!string.IsNullOrEmpty( options.DatabaseDbProxy.Arn))
            Node.SetContext(ContextValues.DatabaseProxyArn, options.DatabaseDbProxy.Arn);
        
        if (!string.IsNullOrEmpty( options.DatabaseDbProxy.Endpoint))
            Node.SetContext(ContextValues.DatabaseProxyEndpoint, options.DatabaseDbProxy.Endpoint);
        
        if (!string.IsNullOrEmpty( options.DatabaseDbProxy.Name))
            Node.SetContext(ContextValues.DatabaseProxyName, options.DatabaseDbProxy.Name);
        
        if (!string.IsNullOrEmpty( options.DatabaseDbProxy.UserName))
            Node.SetContext(ContextValues.DatabaseProxyUser, options.DatabaseDbProxy.UserName);
        
        if (!string.IsNullOrEmpty( options.Data.Storage))
            Node.SetContext(ContextValues.DataStorage, options.Data.Storage);
    }
}