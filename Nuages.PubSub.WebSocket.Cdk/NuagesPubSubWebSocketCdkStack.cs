
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
public class NuagesPubSubWebSocketCdkStack : Stack
{
    public string Asset { get; set; } = "/Users/martin/nuages-io/nuages-pubsub/Nuages.PubSub.Samples.Lambda/bin/Release/net6.0/linux-x64/publish";
        
    public string OnConnectHandler { get; set; } =
        "Nuages.PubSub.Samples.Lambda::Nuages.PubSub.Samples.Lambda.Functions::OnConnectHandlerAsync";

    public string OnDisconnectHandler { get; set; }=
        "Nuages.PubSub.Samples.Lambda::Nuages.PubSub.Samples.Lambda.Functions::OnDisconnectHandlerAsync";
        
    public string OnAuthrorizeHandler { get; set; }=
        "Nuages.PubSub.Samples.Lambda::Nuages.PubSub.Samples.Lambda.Functions::OnAuthorizeHandlerAsync";

    public string SendHandler { get; set; }=
        "Nuages.PubSub.Samples.Lambda::Nuages.PubSub.Samples.Lambda.Functions::SendHandlerAsync";

    public string EchoHandler { get; set; }=
        "Nuages.PubSub.Samples.Lambda::Nuages.PubSub.Samples.Lambda.Functions::EchoHandlerAsync";
        
    public string JoinHandler { get; set; }=
        "Nuages.PubSub.Samples.Lambda::Nuages.PubSub.Samples.Lambda.Functions::JoinHandlerAsync";
        
    public string LeaveHandler { get; set; }=
        "Nuages.PubSub.Samples.Lambda::Nuages.PubSub.Samples.Lambda.Functions::LeaveHandlerAsync";
        
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
        
    public  virtual void BuildTheThing()
    {
        var domainName = (string) Node.TryGetContext("Nuages/PubSub/DomainName");
        var certficateArn = (string) Node.TryGetContext("Nuages/PubSub/CertificateArn");

        var apiGatewayDomainName = CreateApiGatewayDomainName(certficateArn, domainName);

        CreateS3RecordSet(domainName, apiGatewayDomainName);
        
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
            
        //Routes + Integrations
            
        var onConnectRoute = CreateConnectRoute(api, authorizer, onConnectFunction);
        var onDisconnectRoute = CreateRoute("Disconnect", "$disconnect", api, onDisconnectFunction);
        var echoRoute = CreateRoute("Echo", EchoRouteKey, api, echoFunction);
        var sendRoute = CreateRoute("Send", SendROuteKey, api, sendFunction);
        var joinRoute = CreateRoute("Join", JoinRouteKey, api, joinFunction);
        var leavenRoute = CreateRoute("Leave", LeaveRouteKey, api, leaveFunction);

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

        var stage = CreateStage(api, deployment);

        CreatePermission(onConnectFunction);
        CreatePermission(onDisconnectFunction);
        CreatePermission(onAuthorizeFunction);
        CreatePermission(sendFunction);
        CreatePermission(echoFunction);
        CreatePermission(joinFunction);
        CreatePermission(leaveFunction);

        CreateApiMapping(apiGatewayDomainName, api, stage);

        // ReSharper disable once UnusedVariable
        var output = new CfnOutput(this, "NuagesPubSubURI", new CfnOutputProps
        {
            Value = $"wss://{api.Ref}.execute-api.{Aws.REGION}.amazonaws.com/{stage.Ref}",
            Description = "The WSS Protocol URI to connect to"
        });
    }

    private void CreateApiMapping(CfnDomainName apiGatewayDomainName, CfnApi api, CfnStage stage)
    {
        // ReSharper disable once UnusedVariable
        var apiMapping = new CfnApiMapping(this, "NUagesApiMapping", new CfnApiMappingProps
        {
            DomainName = apiGatewayDomainName.DomainName,
            ApiId = api.Ref,
            Stage = stage.Ref
        });
    }

    // private void CreatePermission(string name, CfnApi api, CfnFunction func)
    // {
    //     var permission = new CfnPermission(this, name, new CfnPermissionProps
    //     {
    //         Action = "lambda:InvokeFunction",
    //         FunctionName = func.Ref,
    //         Principal = "apigateway.amazonaws.com"
    //     });
    //         
    //     permission.AddDependsOn(func);
    //     permission.AddDependsOn(api);
    // }
    
    public virtual void CreatePermission(Function func)
    {
        var principal = new ServicePrincipal("apigateway.amazonaws.com");
            
        func.GrantInvoke(principal);
            
        // Equivalent to:
        // fn.AddPermission("my-service Invocation", new Permission {
        //     Principal = principal
        // });
        //
        // var permission = new Permission(this, name, new CfnPermissionProps
        // {
        //     Action = "lambda:InvokeFunction",
        //     FunctionName = func.Ref,
        //     Principal = "apigateway.amazonaws.com"
        // });
        //     
        // permission.AddDependsOn(func);
        // permission.AddDependsOn(api);
    }

    private CfnStage CreateStage(CfnApi api, CfnDeployment deployment)
    {
        return new CfnStage(this, "Stage", new CfnStageProps
        {
            StageName = "Prod",
            ApiId = api.Ref,
            DeploymentId = deployment.Ref
        });
    }


    private CfnRoute CreateConnectRoute(CfnApi api, CfnAuthorizer authorizer, Function onConnectFunction)
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
        
    private CfnRoute CreateRoute(string name, string key, CfnApi api, Function func)
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

    private CfnIntegration CreateIntegration(string name, string apiId, Function func)
    {
        return new CfnIntegration(this, name, new CfnIntegrationProps
        {
            ApiId = apiId,
            IntegrationType = "AWS_PROXY",
            IntegrationUri =
                $"arn:aws:apigateway:{Aws.REGION}:lambda:path/2015-03-31/functions/{func.FunctionArn}/invocations"
        });
    }
        
    private CfnAuthorizer CreateAuthorizer(CfnApi api, Function onAuthorizeFunction)
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

       
    internal NuagesPubSubWebSocketCdkStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
    {
            
    }


    private Function CreateFunction(string name, string handler, Role role, CfnApi api)
    {
        var func = new Function(this, name, new FunctionProps
        {
            Code = Code.FromAsset(Asset),
            Handler = handler,
            Runtime = Runtime.DOTNET_CORE_3_1,
            FunctionName = name,
            MemorySize = 256,
            Role = role,
            Timeout = Duration.Seconds(30),
            Environment = new Dictionary<string, string>
            {
                {"Nuages__PubSub__Region", Aws.REGION},
                {"Nuages__PubSub__Uri", $"wss://{api.Ref}.execute-api.{Aws.REGION}.amazonaws.com/Prod"}
            }
        });

        return func;
    }

    public string NuagesPubSubRole { get; set; } = "NuagesPubSubRole";
    
    private Role CreateRole()
    {
        var role = new Role(this, NuagesPubSubRole, new RoleProps
        {
            AssumedBy = new ServicePrincipal("lambda.amazonaws.com")
        });

        role.AddManagedPolicy(new ManagedPolicy(this, "LambdaBasicExecutionRole", new ManagedPolicyProps
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
            ManagedPolicyName = "LambdaBasicExecutionRole"
        }));
            
        role.AddManagedPolicy(new ManagedPolicy(this, "SystemsManagerParametersRole", new ManagedPolicyProps
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
            ManagedPolicyName = "SystemsManagerParametersRole"
        }));
            
        role.AddManagedPolicy(new ManagedPolicy(this, "ExecuteApiConnectionRole", new ManagedPolicyProps
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
            ManagedPolicyName = "ExecuteApiConnectionRole"
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
        
    private void CreateS3RecordSet( string domainName, CfnDomainName apiGatewayDomainName)
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

    private CfnDomainName CreateApiGatewayDomainName(string certficateArn, string domainName)
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