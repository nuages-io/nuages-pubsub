using Amazon.CDK;
using Amazon.CDK.AWS.Apigatewayv2;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Route53;
using Constructs;

// ReSharper disable VirtualMemberNeverOverridden.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ObjectCreationAsStatement

namespace Nuages.PubSub.Cdk;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable once UnusedType.Global
public partial class NuagesPubSubWebSocketCdkStack<T> : Stack
{
    public string? Asset { get; set; } 
        
    private string? OnConnectHandler { get; set; }
    private string? OnDisconnectHandler { get; set; }
    private string? OnAuthrorizeHandler { get; set; }
    private string? SendHandler { get; set; }
    private string? EchoHandler { get; set; }
    private string? JoinHandler { get; set; }
    private string? LeaveHandler { get; set;}
    
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
    public string SendRouteKey { get; set; } = "send";
    public string JoinRouteKey { get; set; } = "join";
    public string LeaveRouteKey { get; set; } = "leave";
        
    public string StageName { get; set; } = "Prod";
           
    public string NuagesPubSubRole { get; set; } = "Role";
    
    public NuagesPubSubWebSocketCdkStack(Construct scope, string id, IStackProps? props = null) : base(scope, id, props)
    {
        
    }

    public const string ContextDomainName = "Nuages/PubSub/DomainName";
    public const string ContextDomainNameApi = "Nuages/PubSub/DomainNameApi";
    public const string ContextCertificateArn = "Nuages/PubSub/CertificateArn";
    public const string ContextUseCustomDomainName = "Nuages/PubSub/UseCustomDomainName";
    public const string ContextStorage = "Nuages/PubSub/Storage";
    
    public List<CfnRoute> Routes { get; set; } = new ();
    
    // ReSharper disable once UnusedMember.Global
    public  virtual void CreateTemplate()
    {
        NormalizeHandlerName();

        var role = CreateWebSocketRole();
        
        var api = CreateWebSocketApi();

        var onAuthorizeFunction = CreateFunction(OnAuthorizeFunctionName, OnAuthrorizeHandler, role, api);
        var authorizer = CreateAuthorizer(api, onAuthorizeFunction);

        var onConnectFunction = CreateFunction(OnConnectFunctionName, OnConnectHandler, role, api);
        CreateConnectRoute(api, authorizer, onConnectFunction);
        
        var onDisconnectFunction = CreateFunction(OnDisconnectFunctionName, OnDisconnectHandler, role, api);
        CreateRoute("Disconnect", "$disconnect", api, onDisconnectFunction);
        
        var sendFunction = CreateFunction(SendToGroupFunctionName, SendHandler, role, api);
        CreateRoute("Send", SendRouteKey, api, sendFunction);
        
        var echoFunction = CreateFunction(EchoFunctionName, EchoHandler, role, api);
        CreateRoute("Echo", EchoRouteKey, api, echoFunction);
        
        var joinFunction = CreateFunction(JoinFunctionName, JoinHandler, role, api);
        CreateRoute("Join", JoinRouteKey, api, joinFunction);
        
        var leaveFunction = CreateFunction(LeaveFunctionName, LeaveHandler, role, api);
        CreateRoute("Leave", LeaveRouteKey, api, leaveFunction);

        CreateAdditionalFunctionsAndRoutes(api);
        
        var deployment = CreateDeployment(api);

        var stage = CreateStage(api, deployment);

        var useCustomDomain = Convert.ToBoolean(Node.TryGetContext(ContextUseCustomDomainName));

        if (useCustomDomain)
        {
            var domainName = (string) Node.TryGetContext(ContextDomainName);
            Console.WriteLine($"Domain = {domainName}");
            
            var certficateArn = (string) Node.TryGetContext(ContextCertificateArn);
            
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

        var useDynamoDb = Node.TryGetContext(ContextStorage).ToString() == "DynamoDb";
        if (useDynamoDb)
        {
            CreateTables();
        }

        var webSocketUrl = $"wss://{api.Ref}.execute-api.{Aws.REGION}.amazonaws.com/{stage.Ref}";
        
        CreateWebApi(webSocketUrl);
        
        // ReSharper disable once UnusedVariable
        var output = new CfnOutput(this, "NuagesPubSubURI", new CfnOutputProps
        {
            Value = webSocketUrl,
            Description = "The WSS Protocol URI to connect to"
        });
    }

    private void NormalizeHandlerName()
    {
        var type = typeof(T);

        var functionName = $"{Path.GetFileNameWithoutExtension(type.Module.Name)}::{type.Namespace}.{type.Name}";

        if (string.IsNullOrEmpty(OnConnectHandler))
            OnConnectHandler = $"{functionName}::OnConnectHandlerAsync";

        if (string.IsNullOrEmpty(OnDisconnectHandler))
            OnDisconnectHandler = $"{functionName}::OnDisconnectHandlerAsync";

        if (string.IsNullOrEmpty(OnAuthrorizeHandler))
            OnAuthrorizeHandler = $"{functionName}::OnAuthorizeHandlerAsync";

        if (string.IsNullOrEmpty(SendHandler))
            SendHandler = $"{functionName}::SendHandlerAsync";

        if (string.IsNullOrEmpty(EchoHandler))
            EchoHandler = $"{functionName}::EchoHandlerAsync";

        if (string.IsNullOrEmpty(JoinHandler))
            JoinHandler = $"{functionName}::JoinHandlerAsync";

        if (string.IsNullOrEmpty(LeaveHandler))
            LeaveHandler = $"{functionName}::LeaveHandlerAsync";
    }

    // ReSharper disable once UnusedParameter.Global
    protected virtual void CreateAdditionalFunctionsAndRoutes(CfnApi api)
    {
        
    }

    protected virtual CfnDeployment CreateDeployment(CfnApi api)
    {
        var deployment = new CfnDeployment(this, "NuagesPubSubDeployment", new CfnDeploymentProps
        {
            ApiId = api.Ref
        });

        foreach (var route in Routes)
        {
            deployment.AddDependsOn(route);
        }

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

    protected virtual void GrantPermissions(Function func)
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

    protected virtual void CreateConnectRoute(CfnApi api, CfnAuthorizer authorizer, Function onConnectFunction)
    {
        var route = new CfnRoute(this, "ConnectRoute", new CfnRouteProps
        {
            ApiId = api.Ref,
            AuthorizationType = "CUSTOM",
            AuthorizerId = authorizer.Ref,
            OperationName = "ConnectRoute",
            RouteKey = "$connect",
            RouteResponseSelectionExpression = null,
            Target =  Fn.Join("/",  new [] { "integrations",CreateIntegration("ConnectInteg", api.Ref, onConnectFunction).Ref})
        });

        Routes.Add(route);
    }
        
    protected virtual void CreateRoute(string name, string key, CfnApi api, Function func)
    {
        // ReSharper disable once UnusedVariable
        var route = new CfnRoute(this, name + "Route", new CfnRouteProps
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

    protected virtual CfnApi CreateWebSocketApi()
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
        
        if (string.IsNullOrEmpty(Asset))
            throw new Exception("Asset  must be set");
        
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
            },
            Tracing = Tracing.ACTIVE
        });
        

        GrantPermissions(func);
        
        return func;
    }

    protected virtual string GetNormalizedName(string name)
    {
        return $"{StackName}_{name}";
    }
    
    protected virtual Role CreateWebSocketRole()
    {
        var role = new Role(this, NuagesPubSubRole, new RoleProps
        {
            AssumedBy = new ServicePrincipal("lambda.amazonaws.com")
        });

        role.AddManagedPolicy(CreateLambdaBasicExecutionRolePolicy());
            
        role.AddManagedPolicy(CreateExecuteApiConnectionRolePolicy());
        
        role.AddManagedPolicy(CreateDynamoDbRolePolicy());
            
        return role;
    }

    protected virtual ManagedPolicy CreateDynamoDbRolePolicy()
    {
        return new ManagedPolicy(this, GetNormalizedName("DynamoDbRole"), new ManagedPolicyProps
        {
            Document = new PolicyDocument(new PolicyDocumentProps
            {
                Statements = new []{ new PolicyStatement(new PolicyStatementProps
                {
                    Effect = Effect.ALLOW,
                    Actions = new []{"dynamodb:*"},
                    Resources = new []{"*"}
                })}
            }),
            ManagedPolicyName = GetNormalizedName("DynamoDbRole")
        });
    }

    protected virtual ManagedPolicy CreateExecuteApiConnectionRolePolicy()
    {
        return new ManagedPolicy(this, GetNormalizedName("ExecuteApiConnectionRole"), new ManagedPolicyProps
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
        });
    }

    // protected virtual ManagedPolicy CreateSystemsManagerParametersRolePolicy()
    // {
    //     return new ManagedPolicy(this, GetNormalizedName("SystemsManagerParametersRole"), new ManagedPolicyProps
    //     {
    //         Document = new PolicyDocument(new PolicyDocumentProps
    //         {
    //             Statements = new []{ new PolicyStatement(new PolicyStatementProps
    //             {
    //                 Effect = Effect.ALLOW,
    //                 Actions = new []{"ssm:GetParametersByPath"},
    //                 Resources = new []{"*"}
    //             })}
    //         }),
    //         ManagedPolicyName = GetNormalizedName("SystemsManagerParametersRole")
    //     });
    // }

    protected virtual ManagedPolicy CreateLambdaBasicExecutionRolePolicy(string suffix = "")
    {
        return new ManagedPolicy(this, GetNormalizedName("LambdaBasicExecutionRole" + suffix), new ManagedPolicyProps
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
            ManagedPolicyName = GetNormalizedName("LambdaBasicExecutionRole" + suffix)
        });
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
        //var cert = Certificate.FromCertificateArn(this, "NusagePubSubCert", certficateArn);
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

    protected virtual void CreateTables()
    {
        // ReSharper disable once UnusedVariable
        var pubSubConnection = new Table(this, "pub_sub_connection", new TableProps
        {
            TableName = "pub_sub_connection",
            BillingMode = BillingMode.PAY_PER_REQUEST,
            PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
            {
                Name = "Id",
                Type = AttributeType.STRING
            },
            RemovalPolicy= RemovalPolicy.DESTROY
        });
        
        // ReSharper disable once UnusedVariable
        var pubSubAck = new Table(this, "pub_sub_ack", new TableProps
        {
            TableName = "pub_sub_ack",
            BillingMode = BillingMode.PAY_PER_REQUEST,
            PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
            {
                Name = "Id",
                Type = AttributeType.STRING
            },
            RemovalPolicy= RemovalPolicy.DESTROY
        });
        
        // ReSharper disable once UnusedVariable
        var pubSubGroupConnection = new Table(this, "pub_sub_group_connection", new TableProps
        {
            TableName = "pub_sub_group_connection",
            BillingMode = BillingMode.PAY_PER_REQUEST,
            PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
            {
                Name = "Id",
                Type = AttributeType.STRING
            },
            RemovalPolicy= RemovalPolicy.DESTROY
        });
        
        // ReSharper disable once UnusedVariable
        var pubSubGroupUser = new Table(this, "pub_sub_group_user", new TableProps
        {
            TableName = "pub_sub_group_user",
            BillingMode = BillingMode.PAY_PER_REQUEST,
            PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
            {
                Name = "Id",
                Type = AttributeType.STRING
            },
            RemovalPolicy= RemovalPolicy.DESTROY
        });

    }   
}