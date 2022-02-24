using System.Diagnostics.CodeAnalysis;
using Amazon.CDK;
using Constructs;
using Nuages.PubSub.WebSocket.API;

namespace Nuages.PubSub.Cdk.Deploy;

[ExcludeFromCodeCoverage]
public class MyNuagesPubSubStack : NuagesPubSubWebSocketCdkStack<PubSubFunction>
{
    // ReSharper disable once UnusedParameter.Local
    public MyNuagesPubSubStack(Construct scope, string id, IStackProps? props = null) 
        : base(scope, id, props)
    {
        WebSocketAsset = "./src/Nuages.PubSub.WebSocket.API/bin/Release/net6.0/linux-x64/publish";
        ApiAsset = "./src/Nuages.PubSub.API/bin/Release/net6.0/linux-x64/publish";
    }
    
    public void InitializeContextFromOptions(ConfigOptions options)
    {
        if (!string.IsNullOrEmpty(options.WebSocket.Domain))
            Node.SetContext(ContextDomainName,  options.WebSocket.Domain);
        
        if (!string.IsNullOrEmpty(options.WebSocket.CertificateArn))
            Node.SetContext(ContextCertificateArn,options.WebSocket.CertificateArn);
        
        if (!string.IsNullOrEmpty(options.Api.Domain))
            Node.SetContext(ContextDomainNameApi,  options.Api.Domain);
        
        if (!string.IsNullOrEmpty(options.Api.CertificateArn))
            Node.SetContext(ContextCertificateArnApi, options.Api.CertificateArn);
        
        if (!string.IsNullOrEmpty(options.Api.ApiKey))
            Node.SetContext(ContextApiKeyApi, options.Api.ApiKey);

        if (!string.IsNullOrEmpty( options.Env.Auth.Audience))
            Node.SetContext(ContextAudience, options.Env.Auth.Audience);
        
        if (!string.IsNullOrEmpty( options.Env.Auth.Issuer))
            Node.SetContext(ContextIssuer, options.Env.Auth.Issuer);
        
        if (!string.IsNullOrEmpty( options.Env.Auth.Secret))
            Node.SetContext(ContextSecret, options.Env.Auth.Secret);
        
        if (!string.IsNullOrEmpty( options.VpcId))
            Node.SetContext(ContextVpcId, options.VpcId);
        
        if (!string.IsNullOrEmpty( options.DatabaseDbProxy.Arn))
            Node.SetContext(ContextProxyArn, options.DatabaseDbProxy.Arn);
        
        if (!string.IsNullOrEmpty( options.DatabaseDbProxy.Endpoint))
            Node.SetContext(ContextProxyEndpoint, options.DatabaseDbProxy.Endpoint);
        
        if (!string.IsNullOrEmpty( options.DatabaseDbProxy.SecurityGroup))
            Node.SetContext(ContextProxySecurityGroup, options.DatabaseDbProxy.SecurityGroup);
        
        if (!string.IsNullOrEmpty( options.DatabaseDbProxy.Name))
            Node.SetContext(ContextProxyName, options.DatabaseDbProxy.Name);
        
        if (!string.IsNullOrEmpty( options.DatabaseDbProxy.UserName))
            Node.SetContext(ContextProxyUser, options.DatabaseDbProxy.UserName);
        
        if ( options.Data.CreateDynamoDbTables.HasValue)
            Node.SetContext(ContextCreateDynamoDbTables, options.Data.CreateDynamoDbTables.Value.ToString());
        
        if (!string.IsNullOrEmpty( options.Data.Storage))
            Node.SetContext(ContextStorage, options.Data.Storage);
        
        if (options.Data.Port.HasValue)
            Node.SetContext(ContextPort, options.Data.Port.Value.ToString());
        
        if (!string.IsNullOrEmpty( options.Data.ConnectionString))
            Node.SetContext(ContextConnectionString, options.Data.ConnectionString);
        
       
    }

}