
using System;
using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.Apigatewayv2;
using Amazon.CDK.AWS.CertificateManager;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Route53;
using Constructs;
using CfnAuthorizer = Amazon.CDK.AWS.Apigatewayv2.CfnAuthorizer;
using CfnDeployment = Amazon.CDK.AWS.Apigatewayv2.CfnDeployment;
using CfnDeploymentProps = Amazon.CDK.AWS.Apigatewayv2.CfnDeploymentProps;
using CfnStage = Amazon.CDK.AWS.Apigatewayv2.CfnStage;
using CfnStageProps = Amazon.CDK.AWS.Apigatewayv2.CfnStageProps;

// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ObjectCreationAsStatement

namespace Nuages.PubSub.WebSocket.Cdk;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable once UnusedType.Global
public class NuagesPubSubWebSocketCdkStack : Stack
{
    public string Asset { get; set; } = "/Users/martin/nuages-io/nuages-pubsub/Nuages.PubSub.Samples.Lambda/bin/Release/net6.0/linux-x64/publish";
        
    public string? OnConnectHandler { get; set; }
    public string? OnDisconnectHandler { get; set; }
    public string? OnAuthrorizeHandler { get; set; }
    public string? SendHandler { get; set; }
    public string? EchoHandler { get; set; }
    public string? JoinHandler { get; set; }
    public string? LeaveHandler { get; set;}
    
    public string ApiName { get; set; } = "NuagesPubSub";
    
    public string RouteSelectionExpression { get; set; } = "$request.body.type";

    public string[] IdentitySource { get; set; } = { "route.request.querystring.access_token" } ;
    public string AuthorizerName { get; set; } = "Authorizer";

    public string OnConnectFunctionName { get; set; } = "OnConnectFunction";
    public string OnDisconnectFunctionName { get; set; } = "OnDisconnectFunction";
    public string OnAuthorizeFunctionName { get; set; } = "OnAuthorizeFunction";
    public string SendToGroupFunctionName { get; set; } = "SendToGroupFunction";
    public string EchoFunctionName { get; set; } = "EchoFunction";
    public string JoinFunctionName { get; set; } = "JoinFunction";
    public string LeaveFunctionName { get; set; } = "LeaveFunction";
        
    public string EchoRouteKey { get; set; } = "echo";
    public string SendROuteKey { get; set; } = "send";
    public string JoinRouteKey { get; set; } = "join";
    public string LeaveRouteKey { get; set; } = "leave";
        
    public string StageName { get; set; } = "Prod";
           
    public string NuagesPubSubRole { get; set; } = "Role";
    
    public NuagesPubSubWebSocketCdkStack(Construct scope, string id, IStackProps? props = null) : base(scope, id, props)
    {
            
    }

    public const string ContextDomainName = "Nuages/PubSub/DomainName";
    public const string ContextCertificateArn = "Nuages/PubSub/CertificateArn";
    public const string ContextUseCustomDomainName = "Nuages/PubSub/UseCustomDomainName";
    
    // ReSharper disable once UnusedMember.Global
    public  virtual void CreateTemplate()
    {
        var domainName = (string) Node.TryGetContext(ContextDomainName);
        var certficateArn = (string) Node.TryGetContext(ContextCertificateArn);
        var useCustomDomain = Convert.ToBoolean(Node.TryGetContext(ContextUseCustomDomainName));
        
        var role = CreateRole();
        
        var api = CreateApi();

        var onConnectFunction = CreateFunction(OnConnectFunctionName, OnConnectHandler, role, api);
        var onDisconnectFunction = CreateFunction(OnDisconnectFunctionName, OnDisconnectHandler, role, api);
        var onAuthorizeFunction = CreateFunction(OnAuthorizeFunctionName, OnAuthrorizeHandler, role, api);
        var sendFunction = CreateFunction(SendToGroupFunctionName, SendHandler, role, api);
        var echoFunction = CreateFunction(EchoFunctionName, EchoHandler, role, api);
        var joinFunction = CreateFunction(JoinFunctionName, JoinHandler, role, api);
        var leaveFunction = CreateFunction(LeaveFunctionName, LeaveHandler, role, api);

        var authorizer = CreateAuthorizer(api, onAuthorizeFunction);
            
        var onConnectRoute = CreateConnectRoute(api, authorizer, onConnectFunction);
        var onDisconnectRoute = CreateRoute("Disconnect", "$disconnect", api, onDisconnectFunction);
        var echoRoute = CreateRoute("Echo", EchoRouteKey, api, echoFunction);
        var sendRoute = CreateRoute("Send", SendROuteKey, api, sendFunction);
        var joinRoute = CreateRoute("Join", JoinRouteKey, api, joinFunction);
        var leavenRoute = CreateRoute("Leave", LeaveRouteKey, api, leaveFunction);

        var deployment = CreateDeployment(api, onConnectRoute, onDisconnectRoute, echoRoute, sendRoute, joinRoute, leavenRoute);

        var stage = CreateStage(api, deployment);

        CreatePermission(onConnectFunction);
        CreatePermission(onDisconnectFunction);
        CreatePermission(onAuthorizeFunction);
        CreatePermission(sendFunction);
        CreatePermission(echoFunction);
        CreatePermission(joinFunction);
        CreatePermission(leaveFunction);

        if (useCustomDomain)
        {
            var apiGatewayDomainName = CreateApiGatewayDomainName(certficateArn, domainName);
            CreateS3RecordSet(domainName, apiGatewayDomainName);
            CreateApiMapping(apiGatewayDomainName, api, stage);
            
            // ReSharper disable once UnusedVariable
            var output2 = new CfnOutput(this, "NuagesPubSubCustomURI", new CfnOutputProps
            {
                Value = $"wss://{domainName}",
                Description = "The Custom WSS Protocol URI to connect to"
            });
        }

        // ReSharper disable once UnusedVariable
        var output = new CfnOutput(this, "NuagesPubSubURI", new CfnOutputProps
        {
            Value = $"wss://{api.Ref}.execute-api.{Aws.REGION}.amazonaws.com/{stage.Ref}",
            Description = "The WSS Protocol URI to connect to"
        });
    }

    private CfnDeployment CreateDeployment(CfnApi api, CfnRoute onConnectRoute, CfnRoute onDisconnectRoute,
        CfnRoute echoRoute, CfnRoute sendRoute, CfnRoute joinRoute, CfnRoute leavenRoute)
    {
        var deployment = new CfnDeployment(this, "NuagesPubSubDeployment", new CfnDeploymentProps
        {
            ApiId = api.Ref
        });

        deployment.AddDependsOn(onConnectRoute);
        deployment.AddDependsOn(onDisconnectRoute);
        deployment.AddDependsOn(echoRoute);
        deployment.AddDependsOn(sendRoute);
        deployment.AddDependsOn(joinRoute);
        deployment.AddDependsOn(leavenRoute);
        return deployment;
    }

    protected virtual void CreateApiMapping(CfnDomainName apiGatewayDomainName, CfnApi api, CfnStage stage)
    {
        // ReSharper disable once UnusedVariable
        var apiMapping = new CfnApiMapping(this, "NuagesApiMapping", new CfnApiMappingProps
        {
            DomainName = apiGatewayDomainName.DomainName,
            ApiId = api.Ref,
            Stage = stage.Ref
        });
    }

    protected virtual void CreatePermission(Function func)
    {
        var principal = new ServicePrincipal("apigateway.amazonaws.com");
            
        func.GrantInvoke(principal);
    }
    
    protected virtual CfnStage CreateStage(CfnApi api, CfnDeployment deployment)
    {
        return new CfnStage(this, "Stage", new CfnStageProps
        {
            StageName = StageName,
            ApiId = api.Ref,
            DeploymentId = deployment.Ref
        });
    }

    protected virtual CfnRoute CreateConnectRoute(CfnApi api, CfnAuthorizer authorizer, Function onConnectFunction)
    {
        return new CfnRoute(this, "ConnectRoute", new CfnRouteProps
        {
            ApiId = api.Ref,
            AuthorizationType = "CUSTOM",
            AuthorizerId = authorizer.Ref,
            OperationName = "ConnectRoute",
            RouteKey = "$connect",
            RouteResponseSelectionExpression = null,
            Target =  Fn.Join("/",  new [] { "integrations",CreateIntegration("ConnectInteg", api.Ref, onConnectFunction).Ref})
        });
    }
        
    protected virtual CfnRoute CreateRoute(string name, string key, CfnApi api, Function func)
    {
        return new CfnRoute(this, name + "Route", new CfnRouteProps
        {
            ApiId = api.Ref,
            AuthorizationType = "NONE",
            OperationName = name + "Route",
            RouteKey = key,
            RouteResponseSelectionExpression = null,
            Target = Fn.Join("/",  new [] { "integrations", CreateIntegration(name + "Integ", api.Ref, func).Ref}) 
        });
    }

    protected virtual CfnIntegration CreateIntegration(string name, string apiId, Function func)
    {
        return new CfnIntegration(this, name, new CfnIntegrationProps
        {
            ApiId = apiId,
            IntegrationType = "AWS_PROXY",
            IntegrationUri =
                $"arn:aws:apigateway:{Aws.REGION}:lambda:path/2015-03-31/functions/{func.FunctionArn}/invocations"
        });
    }
        
    protected virtual CfnAuthorizer CreateAuthorizer(CfnApi api, Function onAuthorizeFunction)
    {
        var authorizer = new CfnAuthorizer(this, AuthorizerName,
            new CfnAuthorizerProps
            {
                Name = AuthorizerName,
                ApiId = api.Ref,
                AuthorizerType = "REQUEST",
                IdentitySource = IdentitySource,
                AuthorizerUri =
                    $"arn:{Aws.PARTITION}:apigateway:{Aws.REGION}:lambda:path/2015-03-31/functions/{onAuthorizeFunction.FunctionArn}/invocations"
            });

        return authorizer;
    }

    private CfnApi CreateApi()
    {
        var api = new CfnApi(this, ApiName, new CfnApiProps
        {
            ProtocolType = "WEBSOCKET",
            Name = ApiName,
            RouteSelectionExpression = RouteSelectionExpression
        });
        return api;
    }

    protected virtual Function CreateFunction(string name, string? handler, Role role, CfnApi api)
    {
        if (string.IsNullOrEmpty(handler))
            throw new Exception($"Handler for {name} must be set");
        
        var func = new Function(this, name, new FunctionProps
        {
            Code = Code.FromAsset(Asset),
            Handler = handler,
            Runtime = Runtime.DOTNET_CORE_3_1,
            FunctionName = GetNormalizedName(name),
            MemorySize = 256,
            Role = role,
            Timeout = Duration.Seconds(30),
            Environment = new Dictionary<string, string>
            {
                {"Nuages__PubSub__Region", Aws.REGION},
                {"Nuages__PubSub__Uri", $"wss://{api.Ref}.execute-api.{Aws.REGION}.amazonaws.com/{StageName}"}
            }
        });

        return func;
    }

    protected virtual string GetNormalizedName(string name)
    {
        return $"{StackName}_{name}";
    }
    
    protected virtual Role CreateRole()
    {
        var role = new Role(this, NuagesPubSubRole, new RoleProps
        {
            AssumedBy = new ServicePrincipal("lambda.amazonaws.com")
        });

        role.AddManagedPolicy(new ManagedPolicy(this, GetNormalizedName("LambdaBasicExecutionRole"), new ManagedPolicyProps
        {
            Document = new PolicyDocument(new PolicyDocumentProps
            {
                Statements = new []{ new PolicyStatement(new PolicyStatementProps
                {
                    Effect = Effect.ALLOW,
                    Actions = new []{"logs:CreateLogGroup",
                        "logs:CreateLogStream",
                        "logs:PutLogEvents"},
                    Resources = new []{"*"}
                })}
            }),
            ManagedPolicyName = GetNormalizedName("LambdaBasicExecutionRole")
        }));
            
        role.AddManagedPolicy(new ManagedPolicy(this, GetNormalizedName("SystemsManagerParametersRole"), new ManagedPolicyProps
        {
            Document = new PolicyDocument(new PolicyDocumentProps
            {
                Statements = new []{ new PolicyStatement(new PolicyStatementProps
                {
                    Effect = Effect.ALLOW,
                    Actions = new []{"ssm:GetParametersByPath"},
                    Resources = new []{"*"}
                })}
            }),
            ManagedPolicyName = GetNormalizedName("SystemsManagerParametersRole")
        }));
            
        role.AddManagedPolicy(new ManagedPolicy(this, GetNormalizedName("ExecuteApiConnectionRole"), new ManagedPolicyProps
        {
            Document = new PolicyDocument(new PolicyDocumentProps
            {
                Statements = new []{ new PolicyStatement(new PolicyStatementProps
                {
                    Effect = Effect.ALLOW,
                    Actions = new []{"execute-api:ManageConnections"},
                    Resources = new []{"arn:aws:execute-api:*:*:*/@connections/*"}
                })}
            }),
            ManagedPolicyName = GetNormalizedName("ExecuteApiConnectionRole")
        }));
            
        return role;

    }

    protected static string GetBaseDomain(string domainName)
    {
        var tokens = domainName.Split('.');

        if (tokens.Length != 3)
            return domainName;

        var tok  = new List<string>(tokens);
        var remove = tokens.Length - 2;
        tok.RemoveRange(0, remove);

        return tok[0] + "." + tok[1];                                
    }
        
    protected virtual void CreateS3RecordSet( string domainName, CfnDomainName apiGatewayDomainName)
    {
        var hostedZone = HostedZone.FromLookup(this, "Lookup", new HostedZoneProviderProps
        {
            DomainName = GetBaseDomain(domainName)
        });

        // ReSharper disable once UnusedVariable
        var recordSet = new CfnRecordSet(this, "Route53RecordSetGroup", new CfnRecordSetProps
        {
            AliasTarget = new CfnRecordSet.AliasTargetProperty
            {
                DnsName = apiGatewayDomainName.AttrRegionalDomainName,
                HostedZoneId = apiGatewayDomainName.AttrRegionalHostedZoneId
            },
            HostedZoneId = hostedZone.HostedZoneId,
            Name = domainName,
            Type = "A"
           
        });
    }

    protected virtual CfnDomainName CreateApiGatewayDomainName(string certficateArn, string domainName)
    {
        // ReSharper disable once UnusedVariable
        var cert = Certificate.FromCertificateArn(this, "NusagePubSubCert", certficateArn);
        var apiGatewayDomainName = new CfnDomainName(this, "NuagesDomainName", new CfnDomainNameProps
        {
            DomainName = domainName,
            DomainNameConfigurations = new [] { new CfnDomainName.DomainNameConfigurationProperty
            {
                EndpointType = "REGIONAL",
                CertificateArn = certficateArn
                
            }}
            
        });
        return apiGatewayDomainName;
    }
}