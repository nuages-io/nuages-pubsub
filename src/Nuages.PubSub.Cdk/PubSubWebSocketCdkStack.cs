using System.Diagnostics.CodeAnalysis;
using Amazon.CDK;
using Amazon.CDK.AWS.Apigatewayv2;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.RDS;
using Amazon.CDK.AWS.Route53;
using Constructs;
using CfnRoute = Amazon.CDK.AWS.Apigatewayv2.CfnRoute;
using CfnRouteProps = Amazon.CDK.AWS.Apigatewayv2.CfnRouteProps;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable VirtualMemberNeverOverridden.Global
// ReSharper disable MemberCanBeProtected.Global

// ReSharper disable InconsistentNaming
// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable ObjectCreationAsStatement

namespace Nuages.PubSub.Cdk;

[ExcludeFromCodeCoverage]
public partial class PubSubWebSocketCdkStack<T> : Stack
{
    protected string? WebSocketAsset { get; set; }

    private string? OnConnectHandler { get; set; }
    private string? OnDisconnectHandler { get; set; }
    private string? OnAuthrorizeHandler { get; set; }
    private string? SendHandler { get; set; }
    private string? EchoHandler { get; set; }
    private string? JoinHandler { get; set; }
    private string? LeaveHandler { get; set; }

    public string ApiNameWebSocket { get; set; } = "WebSocket";

    public string RouteSelectionExpression { get; set; } = "$request.body.type";

    public string[] IdentitySource { get; set; } = { "route.request.querystring.access_token" };
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

    public string StageName { get; set; } = "prod";

    public string WebApiHandler { get; set; } =
        "Nuages.PubSub.API::Nuages.PubSub.API.LambdaEntryPoint::FunctionHandlerAsync";
    
    public string NuagesPubSubRole { get; set; } = "Role";

    protected IVpc? _vpc;
    private ISecurityGroup? _proxySg;
    private IDatabaseProxy? _proxy;
    
    private IDatabaseProxy? Proxy
    {
        get
        {
            if (!string.IsNullOrEmpty(DatabaseProxyArn))
            {
                if (string.IsNullOrEmpty(DatabaseProxyName))
                    throw new Exception("ProxyName is required");

                if (string.IsNullOrEmpty(DatabaseProxyEndpoint))
                    throw new Exception("ProxyEndpoint is required");
                
                if (string.IsNullOrEmpty(DatabaseProxySecurityGroup))
                    throw new Exception("ProxySecurityGroup is required");

                _proxy ??= DatabaseProxy.FromDatabaseProxyAttributes(this, MakeId("Proxy"), new DatabaseProxyAttributes
                {
                    DbProxyArn = DatabaseProxyArn,
                    DbProxyName = DatabaseProxyName,
                    Endpoint = DatabaseProxyEndpoint,
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
                _proxySg = SecurityGroup.FromLookupById(this, "WebApiSGDefault", DatabaseProxySecurityGroup!);

            return _proxySg;
        }
    }

    private IVpc? CurrentVpc
    {
        get
        {
            if (!string.IsNullOrEmpty(VpcId))
            {
                _vpc ??= Vpc.FromLookup(this, "Vpc", new VpcLookupOptions
                {
                    VpcId = VpcId
                });
            }

            return _vpc;
        }
    }

    private ISecurityGroup? _vpcSecurityGroup;
    
    private ISecurityGroup[]? VpcSecurityGroups
    {
        get
        {
            if (!string.IsNullOrEmpty(VpcId))
            {
                _vpcSecurityGroup ??= CreateVpcSecurityGroup();
            }

            return _vpcSecurityGroup != null ? new[] { _vpcSecurityGroup, ProxySg } : null;
        }
    }

    protected virtual SecurityGroup CreateVpcSecurityGroup()
    {
        return new SecurityGroup(this, MakeId("WSSSecurityGroup"), new SecurityGroupProps
        {
            Vpc = CurrentVpc!,
            AllowAllOutbound = true,
            Description = "PubSub WSS Security Group"
        });
    }

    protected PubSubWebSocketCdkStack(Construct scope, string id, IStackProps? props = null) : base(scope, id, props)
    {
        
    }

    
    public string? WebSocketDomainName { get; set; }
    public string? WebSocketCertificateArn { get; set; }

    public string? ApiDomainName { get; set; }
    public string? ApiCertificateArn { get; set; }
    public string? ApiApiKey { get; set; }
    
    public string? AuthIssuer { get; set; }
    public string? AuthAudience { get; set; }
    public string? AuthSecret { get; set; }
    public string? VpcId { get; set; }
    
    public string? DatabaseProxyArn { get; set; }
    public string? DatabaseProxyName { get; set; }
    public string? DatabaseProxySecurityGroup { get; set; }
    public string? DatabaseProxyEndpoint { get; set; }
    public string? DatabaseProxyUser { get; set; }
    
    public string? DataStorage { get; set; }
    public string? DataConnectionString { get; set; }
    public int? DataPort { get; set; }
    
    public List<CfnRoute> Routes { get; set; } = new();

    protected virtual string MakeId(string id)
    {
        return $"{StackName}-{id}";
    }

    // ReSharper disable once UnusedMember.Global
    public virtual void CreateTemplate()
    {
        NormalizeHandlerName();
        
        ReadContextVariables();
        
        var role = CreateWebSocketRole();

        var api = CreateWebSocketApi();
        
        var onAuthorizeFunction = CreateWebSocketFunction(OnAuthorizeFunctionName, OnAuthrorizeHandler, role, api);
        var authorizer = CreateAuthorizer(api, onAuthorizeFunction);

        var onConnectFunction = CreateWebSocketFunction(OnConnectFunctionName, OnConnectHandler, role, api);
        CreateConnectRoute(api, authorizer, onConnectFunction);

        var onDisconnectFunction = CreateWebSocketFunction(OnDisconnectFunctionName, OnDisconnectHandler, role, api);
        CreateRoute("Disconnect", "$disconnect", api, onDisconnectFunction);

        var sendFunction = CreateWebSocketFunction(SendToGroupFunctionName, SendHandler, role, api);
        CreateRoute("Send", SendRouteKey, api, sendFunction);

        var echoFunction = CreateWebSocketFunction(EchoFunctionName, EchoHandler, role, api);
        CreateRoute("Echo", EchoRouteKey, api, echoFunction);

        var joinFunction = CreateWebSocketFunction(JoinFunctionName, JoinHandler, role, api);
        CreateRoute("Join", JoinRouteKey, api, joinFunction);

        var leaveFunction = CreateWebSocketFunction(LeaveFunctionName, LeaveHandler, role, api);
        CreateRoute("Leave", LeaveRouteKey, api, leaveFunction);

        CreateAdditionalFunctionsAndRoutes(api);

        var deployment = CreateDeployment(api);

        var stage = CreateStage(api, deployment);

        if (!string.IsNullOrEmpty(WebSocketDomainName))
        {
            var apiGatewayDomainName = CreateApiGatewayDomainName(WebSocketCertificateArn!, WebSocketDomainName);
            CreateS3RecordSet(WebSocketDomainName, apiGatewayDomainName);
            CreateApiMapping(apiGatewayDomainName, api, stage);

            // ReSharper disable once UnusedVariable
            var output2 = new CfnOutput(this, "NuagesPubSubCustomURI", new CfnOutputProps
            {
                Value = $"wss://{WebSocketDomainName}",
                Description = "The Custom WSS Protocol URI to connect to"
            });
        }

        CreateTables();

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
        var apiMapping = new CfnApiMapping(this, MakeId("NuagesApiMapping"), new CfnApiMappingProps
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
            RouteKey = "$connect",
            RouteResponseSelectionExpression = null,
            Target = Fn.Join("/",
                new[] { "integrations", CreateIntegration("ConnectInteg", api.Ref, onConnectFunction).Ref })
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
            RouteKey = key,
            RouteResponseSelectionExpression = null,
            Target = Fn.Join("/", new[] { "integrations", CreateIntegration(name + "Integ", api.Ref, func).Ref })
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
        var name = MakeId(AuthorizerName);

        var authorizer = new CfnAuthorizer(this, name,
            new CfnAuthorizerProps
            {
                Name = name,
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
        var name = MakeId(ApiNameWebSocket);

        var api = new CfnApi(this, name, new CfnApiProps
        {
            ProtocolType = "WEBSOCKET",
            Name = name,
            RouteSelectionExpression = RouteSelectionExpression
        });
        return api;
    }

    protected virtual Function CreateWebSocketFunction(string name, string? handler, Role role, CfnApi api)
    {
        if (string.IsNullOrEmpty(handler))
            throw new Exception($"Handler for {name} must be set");

        if (string.IsNullOrEmpty(WebSocketAsset))
            throw new Exception("Asset  must be set");

        var func = new Function(this, name, new FunctionProps
        {
            Code = Code.FromAsset(WebSocketAsset),
            Handler = handler,
            Runtime = new Runtime("dotnet6"),
            MemorySize = 512,
            Role = role,
            Timeout = Duration.Seconds(30),
            Environment = GetEnvVariables(api),
            Tracing = Tracing.ACTIVE,
            Vpc = CurrentVpc,
            SecurityGroups = VpcSecurityGroups,
            AllowPublicSubnet = true
        });

        GrantPermissions(func);
        
        if (Proxy != null )
        {
            Proxy.GrantConnect(func, DatabaseProxyUser);
            
            if (VpcSecurityGroups != null)
            {
                var port = GetPort();
                
                // if (port.HasValue)
                //     ProxySg.AddIngressRule(VpcSecurityGroups.First(), Port.Tcp(port.Value), "PubSub WSS MySql");
            }
        }
        
        return func;
    }

    private Dictionary<string, string> GetEnvVariables(CfnApi api)
    {
        var variables =  new Dictionary<string, string>
        {
            { "Nuages__PubSub__Region", Aws.REGION },
            { "Nuages__PubSub__Uri", $"wss://{api.Ref}.execute-api.{Aws.REGION}.amazonaws.com/{StageName}" },
            { "Nuages__PubSub__StackName", StackName }
        };

        if (!string.IsNullOrEmpty(AuthIssuer))
            variables.Add("Nuages__PubSub__Issuer", AuthIssuer);
        
        if (!string.IsNullOrEmpty(AuthAudience))
            variables.Add("Nuages__PubSub__Audience", AuthAudience);
        
        if (!string.IsNullOrEmpty(AuthSecret))
            variables.Add("Nuages__PubSub__Secret", AuthSecret);

        if (!string.IsNullOrEmpty(DataStorage))
        {
            variables.Add("Nuages__Data__Storage", DataStorage);
        
            if (!string.IsNullOrEmpty(DataConnectionString))
                variables.Add($"Nuages__Data__{DataStorage}__ConnectionString", DataConnectionString);
        }
            
        return variables;
    }

    private double? GetPort()
    {
        return DataStorage switch
        {
            "MySql" => 3306,
            _ => null
        };
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
        role.AddManagedPolicy(CreateSystemsManagerPolicy());

        return role;
    }

    protected virtual ManagedPolicy CreateDynamoDbRolePolicy(string suffix = "")
    {
        return new ManagedPolicy(this, MakeId("DynamoDbRole" + suffix), new ManagedPolicyProps
        {
            Document = new PolicyDocument(new PolicyDocumentProps
            {
                Statements = new[]
                {
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Effect = Effect.ALLOW,
                        Actions = new[] { "dynamodb:*" },
                        Resources = new[] { "*" }
                    })
                }
            })
        });
    }

    protected virtual ManagedPolicy CreateExecuteApiConnectionRolePolicy(string suffix = "")
    {
        return new ManagedPolicy(this, MakeId("ExecuteApiConnectionRole" + suffix), new ManagedPolicyProps
        {
            Document = new PolicyDocument(new PolicyDocumentProps
            {
                Statements = new[]
                {
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Effect = Effect.ALLOW,
                        Actions = new[] { "execute-api:ManageConnections" },
                        Resources = new[] { "arn:aws:execute-api:*:*:*/@connections/*" }
                    })
                }
            })
        });
    }

    protected virtual ManagedPolicy CreateSystemsManagerPolicy(string suffix = "")
    {
        return new ManagedPolicy(this, MakeId("SystemsManagerParametersRole" + suffix), new ManagedPolicyProps
        {
            Document = new PolicyDocument(new PolicyDocumentProps
            {
                Statements = new[]
                {
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Effect = Effect.ALLOW,
                        Actions = new[] { "ssm:GetParametersByPath", "appconfig:GetConfiguration" },
                        Resources = new[] { "*" }
                    })
                }
            })
        });
    }

    protected virtual ManagedPolicy CreateLambdaBasicExecutionRolePolicy(string suffix = "")
    {
        var permissions = new []
        {
            "logs:CreateLogGroup",
            "logs:CreateLogStream",
            "logs:PutLogEvents"
        };
        
        if (CurrentVpc != null)
        {
            var p = new []
            {
                "logs:CreateLogGroup",
                "logs:CreateLogStream",
                "logs:PutLogEvents",
                "ec2:CreateNetworkInterface",
                "ec2:DescribeNetworkInterfaces",
                "ec2:DeleteNetworkInterface",
                "ec2:AssignPrivateIpAddresses",
                "ec2:UnassignPrivateIpAddresses"
            };

            permissions = permissions.Union(p).ToArray();
        }
        
        return new ManagedPolicy(this, MakeId("LambdaBasicExecutionRole" + suffix), new ManagedPolicyProps
        {
            Document = new PolicyDocument(new PolicyDocumentProps
            {
                Statements = new[]
                {
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Effect = Effect.ALLOW,
                        Actions = permissions,
                        Resources = new[] { "*" }
                    })
                }
            })
        });
    }

    protected static string GetBaseDomain(string domainName)
    {
        var tokens = domainName.Split('.');

        if (tokens.Length != 3)
            return domainName;

        var tok = new List<string>(tokens);
        var remove = tokens.Length - 2;
        tok.RemoveRange(0, remove);

        return tok[0] + "." + tok[1];
    }

    protected virtual void CreateS3RecordSet(string domainName, CfnDomainName apiGatewayDomainName)
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
        Console.WriteLine($"certficateArn = {certficateArn}");
        // ReSharper disable once UnusedVariable
        var apiGatewayDomainName = new CfnDomainName(this, "NuagesDomainName", new CfnDomainNameProps
        {
            DomainName = domainName,
            DomainNameConfigurations = new[]
            {
                new CfnDomainName.DomainNameConfigurationProperty
                {
                    EndpointType = "REGIONAL",
                    CertificateArn = certficateArn
                }
            }
        });
        return apiGatewayDomainName;
    }

    private void ReadContextVariables()
    {
        WebSocketDomainName = Node.TryGetContext(ContextValues.WebSocketDomain) != null!
            ? Node.TryGetContext(ContextValues.WebSocketDomain).ToString()
            : null;
        
        WebSocketCertificateArn = Node.TryGetContext(ContextValues.WebSocketCertificateArn) != null!
            ? Node.TryGetContext(ContextValues.WebSocketCertificateArn).ToString()
            : null;
        
        ApiDomainName = Node.TryGetContext(ContextValues.ApiDomain) != null!
            ? Node.TryGetContext(ContextValues.ApiDomain).ToString()
            : null;
        
        ApiCertificateArn = Node.TryGetContext(ContextValues.ApiCertificateArn) != null!
            ? Node.TryGetContext(ContextValues.ApiCertificateArn).ToString()
            : null;
        
        ApiApiKey = Node.TryGetContext(ContextValues.ApiApiKey) != null!
            ? Node.TryGetContext(ContextValues.ApiApiKey).ToString()
            : null;
        
        AuthIssuer = Node.TryGetContext(ContextValues.AuthIssuer) != null!
            ? Node.TryGetContext(ContextValues.AuthIssuer).ToString()
            : null;
        
        AuthIssuer = Node.TryGetContext(ContextValues.AuthIssuer) != null!
            ? Node.TryGetContext(ContextValues.AuthIssuer).ToString()
            : null;

        AuthAudience = Node.TryGetContext(ContextValues.AuthAudience) != null!
            ? Node.TryGetContext(ContextValues.AuthAudience).ToString()
            : null;

        AuthSecret = Node.TryGetContext(ContextValues.AuthSecret) != null!
            ? Node.TryGetContext(ContextValues.AuthSecret).ToString()
            : null;

        VpcId = Node.TryGetContext(ContextValues.VpcId) != null!
            ? Node.TryGetContext(ContextValues.VpcId).ToString()
            : null;

        DatabaseProxyArn = Node.TryGetContext(ContextValues.DatabaseProxyArn) != null!
            ? Node.TryGetContext(ContextValues.DatabaseProxyArn).ToString()
            : null;

        DatabaseProxyEndpoint = Node.TryGetContext(ContextValues.DatabaseProxyEndpoint) != null!
            ? Node.TryGetContext(ContextValues.DatabaseProxyEndpoint).ToString()
            : null;

        DatabaseProxyName = Node.TryGetContext(ContextValues.DatabaseProxyName) != null!
            ? Node.TryGetContext(ContextValues.DatabaseProxyName).ToString()
            : null;

        DatabaseProxyUser = Node.TryGetContext(ContextValues.DatabaseProxyUser) != null!
            ? Node.TryGetContext(ContextValues.DatabaseProxyUser).ToString()
            : null;

        DatabaseProxySecurityGroup = Node.TryGetContext(ContextValues.DatabaseProxySecurityGroupId) != null!
            ? Node.TryGetContext(ContextValues.DatabaseProxySecurityGroupId).ToString()
            : null;

        DataStorage = Node.TryGetContext(ContextValues.DataStorage) != null!
            ? Node.TryGetContext(ContextValues.DataStorage).ToString()
            : null;

        DataConnectionString = Node.TryGetContext(ContextValues.DataConnectionString) != null!
            ? Node.TryGetContext(ContextValues.DataConnectionString).ToString()
            : null;

        DataPort = Node.TryGetContext(ContextValues.DataPort) != null!
            ? Convert.ToInt32(Node.TryGetContext(ContextValues.DataPort).ToString())
            : null;
    }

 
}