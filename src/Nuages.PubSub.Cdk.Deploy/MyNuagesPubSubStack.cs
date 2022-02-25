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
            Node.SetContext(ContextValues.WebSocketDomain,  options.WebSocket.Domain);
        
        if (!string.IsNullOrEmpty(options.WebSocket.CertificateArn))
            Node.SetContext(ContextValues.WebSocketCertificateArn,options.WebSocket.CertificateArn);
        
        if (!string.IsNullOrEmpty(options.Api.Domain))
            Node.SetContext(ContextValues.ApiDomain,  options.Api.Domain);
        
        if (!string.IsNullOrEmpty(options.Api.CertificateArn))
            Node.SetContext(ContextValues.ApiCertificateArn, options.Api.CertificateArn);
        
        if (!string.IsNullOrEmpty(options.Api.ApiKey))
            Node.SetContext(ContextValues.ApiApiKey, options.Api.ApiKey);

        if (!string.IsNullOrEmpty( options.Env.Auth.Audience))
            Node.SetContext(ContextValues.AuthAudience, options.Env.Auth.Audience);
        
        if (!string.IsNullOrEmpty( options.Env.Auth.Issuer))
            Node.SetContext(ContextValues.AuthIssuer, options.Env.Auth.Issuer);
        
        if (!string.IsNullOrEmpty( options.Env.Auth.Secret))
            Node.SetContext(ContextValues.AuthSecret, options.Env.Auth.Secret);
        
        if (!string.IsNullOrEmpty( options.VpcId))
            Node.SetContext(ContextValues.VpcId, options.VpcId);
        
        if (!string.IsNullOrEmpty( options.DatabaseDbProxy.Arn))
            Node.SetContext(ContextValues.DatabaseProxyArn, options.DatabaseDbProxy.Arn);
        
        if (!string.IsNullOrEmpty( options.DatabaseDbProxy.Endpoint))
            Node.SetContext(ContextValues.DatabaseProxyEndpoint, options.DatabaseDbProxy.Endpoint);
        
        if (!string.IsNullOrEmpty( options.DatabaseDbProxy.SecurityGroup))
            Node.SetContext(ContextValues.DatabaseProxySecurityGroupId, options.DatabaseDbProxy.SecurityGroup);
        
        if (!string.IsNullOrEmpty( options.DatabaseDbProxy.Name))
            Node.SetContext(ContextValues.DatabaseProxyName, options.DatabaseDbProxy.Name);
        
        if (!string.IsNullOrEmpty( options.DatabaseDbProxy.UserName))
            Node.SetContext(ContextValues.DatabaseProxyUser, options.DatabaseDbProxy.UserName);
        
        if (!string.IsNullOrEmpty( options.Data.Storage))
            Node.SetContext(ContextValues.DataStorage, options.Data.Storage);
        
        if (options.Data.Port.HasValue)
            Node.SetContext(ContextValues.DataPort, options.Data.Port.Value.ToString());
        
        if (!string.IsNullOrEmpty( options.Data.ConnectionString))
            Node.SetContext(ContextValues.DataConnectionString, options.Data.ConnectionString);
        
       
    }

}