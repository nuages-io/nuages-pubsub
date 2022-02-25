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
            Node.SetContext(ContextWebSocketDomainName,  options.WebSocket.Domain);
        
        if (!string.IsNullOrEmpty(options.WebSocket.CertificateArn))
            Node.SetContext(ContextWebSocketCertificateArn,options.WebSocket.CertificateArn);
        
        if (!string.IsNullOrEmpty(options.Api.Domain))
            Node.SetContext(ContextApiDomainName,  options.Api.Domain);
        
        if (!string.IsNullOrEmpty(options.Api.CertificateArn))
            Node.SetContext(ContextApiCertificateArn, options.Api.CertificateArn);
        
        if (!string.IsNullOrEmpty(options.Api.ApiKey))
            Node.SetContext(ContextApiApiKey, options.Api.ApiKey);

        if (!string.IsNullOrEmpty( options.Env.Auth.Audience))
            Node.SetContext(ContextAuthAudience, options.Env.Auth.Audience);
        
        if (!string.IsNullOrEmpty( options.Env.Auth.Issuer))
            Node.SetContext(ContextAuthIssuer, options.Env.Auth.Issuer);
        
        if (!string.IsNullOrEmpty( options.Env.Auth.Secret))
            Node.SetContext(ContextAuthSecret, options.Env.Auth.Secret);
        
        if (!string.IsNullOrEmpty( options.VpcId))
            Node.SetContext(ContextVpcId, options.VpcId);
        
        if (!string.IsNullOrEmpty( options.DatabaseDbProxy.Arn))
            Node.SetContext(ContextDatabaseProxyArn, options.DatabaseDbProxy.Arn);
        
        if (!string.IsNullOrEmpty( options.DatabaseDbProxy.Endpoint))
            Node.SetContext(ContextDatabaseProxyEndpoint, options.DatabaseDbProxy.Endpoint);
        
        if (!string.IsNullOrEmpty( options.DatabaseDbProxy.SecurityGroup))
            Node.SetContext(ContextDatabaseProxySecurityGroup, options.DatabaseDbProxy.SecurityGroup);
        
        if (!string.IsNullOrEmpty( options.DatabaseDbProxy.Name))
            Node.SetContext(ContextDatabaseProxyName, options.DatabaseDbProxy.Name);
        
        if (!string.IsNullOrEmpty( options.DatabaseDbProxy.UserName))
            Node.SetContext(ContextDatabaseProxyUser, options.DatabaseDbProxy.UserName);
        
        if ( options.Data.CreateDynamoDbTables.HasValue)
            Node.SetContext(ContextDataCreateDynamoDbTables, options.Data.CreateDynamoDbTables.Value.ToString());
        
        if (!string.IsNullOrEmpty( options.Data.Storage))
            Node.SetContext(ContextDataStorage, options.Data.Storage);
        
        if (options.Data.Port.HasValue)
            Node.SetContext(ContextDataPort, options.Data.Port.Value.ToString());
        
        if (!string.IsNullOrEmpty( options.Data.ConnectionString))
            Node.SetContext(ContextDataConnectionString, options.Data.ConnectionString);
        
       
    }

}