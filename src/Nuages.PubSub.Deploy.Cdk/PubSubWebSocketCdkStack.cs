using System.Diagnostics.CodeAnalysis;
using Amazon.CDK;
using Amazon.CDK.AWS.Apigatewayv2;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.RDS;
using Amazon.CDK.AWS.Route53;
using Constructs;
using Nuages.PubSub.WebSocket;
using CfnRoute = Amazon.CDK.AWS.Apigatewayv2.CfnRoute;
using CfnRouteProps = Amazon.CDK.AWS.Apigatewayv2.CfnRouteProps;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable VirtualMemberNeverOverridden.Global
// ReSharper disable MemberCanBeProtected.Global

// ReSharper disable InconsistentNaming
// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable ObjectCreationAsStatement

namespace Nuages.PubSub.Deploy.Cdk;

[ExcludeFromCodeCoverage]
public partial class PubSubWebSocketCdkStack : Stack
{
    
    public static void CreateStack(Construct scope, ConfigOptions options, RuntimeOptions runtimeOptions)
    {
        var stack = new PubSubWebSocketCdkStack(scope, options.StackName, new StackProps
        {
            StackName = options.StackName,
            Env = new Amazon.CDK.Environment
            {
                Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION")
            }
        })
        {
            ConfigOptions = options,
            RuntimeOptions = runtimeOptions
        };
        
        stack.BuildStack();
    }
    
    protected string? WebSocketAsset { get; set; }
   
    private string? OnConnectHandler { get; set; }
    private string? OnDisconnectHandler { get; set; }
    private string? OnAuthrorizeHandler { get; set; }
    private string? SendHandler { get; set; }
    private string? EchoHandler { get; set; }
    private string? JoinHandler { get; set; }
    private string? LeaveHandler { get; set; }

    public string? ApiRef { get; set; }
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

    public string WebApiHandler { get; set; }
    
    public string NuagesPubSubRole { get; set; } = "Role";
    
    protected ConfigOptions ConfigOptions { get; set; } = new();
    protected RuntimeOptions RuntimeOptions { get; set; } = new();
    
    public List<CfnRoute> Routes { get; set; } = new();

    protected string MakeId(string id)
    {
        return $"{StackName}-{id}";
    }
    
    protected IVpc? _vpc;
   
    private IDatabaseProxy? _proxy;
    
    private ISecurityGroup? _securityGroup;
    private ISecurityGroup? _vpcSecurityGroup;
    
    protected PubSubWebSocketCdkStack(Construct scope, string id, IStackProps? props = null) : base(scope, id, props)
    {
        WebSocketAsset = "./src/Nuages.PubSub.WebSocket/bin/Release/net6.0/linux-x64/publish";
        ApiAsset = "./src/Nuages.PubSub.API/bin/Release/net6.0/linux-x64/publish";
        WebApiHandler = "Nuages.PubSub.API";
    }
    
    private IDatabaseProxy? Proxy
    {
        get
        {
            if (!string.IsNullOrEmpty(ConfigOptions.DatabaseDbProxy.Arn))
            {
                if (string.IsNullOrEmpty(ConfigOptions.DatabaseDbProxy.Name))
                    throw new Exception("ProxyName is required");

                if (string.IsNullOrEmpty(ConfigOptions.DatabaseDbProxy.Endpoint))
                    throw new Exception("ProxyEndpoint is required");
                
                if (string.IsNullOrEmpty(ConfigOptions.SecurityGroupId))
                    throw new Exception("SecurityGroup is required");

                _proxy ??= DatabaseProxy.FromDatabaseProxyAttributes(this, MakeId("Proxy"), new DatabaseProxyAttributes
                {
                    DbProxyArn = ConfigOptions.DatabaseDbProxy.Arn,
                    DbProxyName = ConfigOptions.DatabaseDbProxy.Name,
                    Endpoint = ConfigOptions.DatabaseDbProxy.Endpoint,
                    SecurityGroups = new[] { SecurityGroup! }
                });

            }
           
            return _proxy;
        }
    }

    private IVpc? CurrentVpc
    {
        get
        {
            if (!string.IsNullOrEmpty(ConfigOptions.VpcId) && _vpc == null)
            {
                Console.WriteLine("Vpc.FromLookup");
                _vpc = Vpc.FromLookup(this, "Vpc", new VpcLookupOptions
                {
                    VpcId = ConfigOptions.VpcId
                });
            }

            return _vpc;
        }
    }
    
    private ISecurityGroup? SecurityGroup
    {
        get
        {
            if (_securityGroup == null && !string.IsNullOrEmpty(ConfigOptions.SecurityGroupId))
                _securityGroup = Amazon.CDK.AWS.EC2.SecurityGroup.FromLookupById(this, "WebApiSGDefault", ConfigOptions.SecurityGroupId!);

            return _securityGroup;
        }
    }
    
    private ISecurityGroup[] SecurityGroups
    {
        get
        {
            if (_vpcSecurityGroup == null && !string.IsNullOrEmpty(ConfigOptions.VpcId))
            {
                _vpcSecurityGroup ??= CreateVpcSecurityGroup();
            }

            var list = new List<ISecurityGroup>();
            
            if (_vpcSecurityGroup != null)
                list.Add(_vpcSecurityGroup);

            if (SecurityGroup != null)
                list.Add(SecurityGroup);

            return list.ToArray();
        }
    }

    protected SecurityGroup CreateVpcSecurityGroup()
    {
        return new SecurityGroup(this, MakeId("WSSSecurityGroup"), new SecurityGroupProps
        {
            Vpc = CurrentVpc!,
            AllowAllOutbound = true,
            Description = "PubSub WSS Security Group"
        });
    }

    
    // ReSharper disable once UnusedMember.Global
    public void BuildStack()
    {
        NormalizeHandlerName();
        
        var api = CreateWebSocketApi();

        ApiRef = api.Ref;
        
        var role = CreateWebSocketRole();
        
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

        var deployment = CreateDeployment(api);

        var stage = CreateStage(api, deployment);

        if (!string.IsNullOrEmpty(ConfigOptions.WebSocket.Domain))
        {
            var apiGatewayDomainName = CreateApiGatewayDomainName(ConfigOptions.WebSocket.CertificateArn!, ConfigOptions.WebSocket.Domain);
            CreateS3RecordSet(ConfigOptions.WebSocket.Domain, apiGatewayDomainName);
            CreateApiMapping(apiGatewayDomainName, api, stage);

            // ReSharper disable once UnusedVariable
            var output2 = new CfnOutput(this, "NuagesPubSubCustomURI", new CfnOutputProps
            {
                Value = $"wss://{ConfigOptions.WebSocket.Domain}",
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
        var type = typeof(PubSubFunction);

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

    protected CfnDeployment CreateDeployment(CfnApi api)
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

    protected void CreateApiMapping(CfnDomainName apiGatewayDomainName, CfnApi api, CfnStage stage)
    {
        Console.WriteLine($"CreateApiMapping domainName = {apiGatewayDomainName.DomainName}");
        
        // ReSharper disable once UnusedVariable
        var apiMapping = new CfnApiMapping(this, MakeId("NuagesApiMapping"), new CfnApiMappingProps
        {
            DomainName = apiGatewayDomainName.DomainName,
            ApiId = api.Ref,
            Stage = stage.Ref
        });
    }

    protected static void GrantPermissions(Function func)
    {
        var principal = new ServicePrincipal("apigateway.amazonaws.com");

        func.GrantInvoke(principal);
    }

    protected CfnStage CreateStage(CfnApi api, CfnDeployment deployment)
    {
        return new CfnStage(this, "Stage", new CfnStageProps
        {
            StageName = StageName,
            ApiId = api.Ref,
            DeploymentId = deployment.Ref
        });
    }

    protected void CreateConnectRoute(CfnApi api, CfnAuthorizer authorizer, Function onConnectFunction)
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

    protected void CreateRoute(string name, string key, CfnApi api, Function func)
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

    protected CfnIntegration CreateIntegration(string name, string apiId, Function func)
    {
        return new CfnIntegration(this, name, new CfnIntegrationProps
        {
            ApiId = apiId,
            IntegrationType = "AWS_PROXY",
            IntegrationUri =
                $"arn:aws:apigateway:{Aws.REGION}:lambda:path/2015-03-31/functions/{func.FunctionArn}/invocations"
        });
    }

    protected CfnAuthorizer CreateAuthorizer(CfnApi api, Function onAuthorizeFunction)
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

    protected CfnApi CreateWebSocketApi()
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

    protected Function CreateWebSocketFunction(string name, string? handler, Role role, CfnApi api)
    {
        if (string.IsNullOrEmpty(handler))
            throw new Exception($"Handler for {name} must be set");

        if (string.IsNullOrEmpty(WebSocketAsset))
            throw new Exception("Asset  must be set");

        var func = new Function(this, name, new FunctionProps
        {
            Code = Code.FromAsset(WebSocketAsset),
            Handler = handler,
            Runtime = Runtime.DOTNET_6,
            MemorySize = 512,
            Role = role,
            Timeout = Duration.Seconds(30),
            Environment = GetEnvVariables(api),
            Tracing = Tracing.ACTIVE,
            Vpc = CurrentVpc,
            SecurityGroups = SecurityGroups,
            AllowPublicSubnet = true
        });

        GrantPermissions(func);

        Proxy?.GrantConnect(func, ConfigOptions.DatabaseDbProxy.UserName);

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

        GetSharedEnvVariables(variables);

        return variables;
    }

    private void GetSharedEnvVariables(Dictionary<string, string> variables)
    {
        if (!string.IsNullOrEmpty(RuntimeOptions.Auth.Audience))
            variables.Add("Nuages__PubSub__Auth__Audience", RuntimeOptions.Auth.Audience);

        if (!string.IsNullOrEmpty(RuntimeOptions.Auth.Issuer))
            variables.Add("Nuages__PubSub__Auth__Issuer", RuntimeOptions.Auth.Issuer);

        if (!string.IsNullOrEmpty(RuntimeOptions.Auth.Secret))
            variables.Add("Nuages__PubSub__Auth__Secret", RuntimeOptions.Auth.Secret);

        if (!string.IsNullOrEmpty(RuntimeOptions.Data.Storage))
            variables.Add("Nuages__PubSub__Data__Storage", RuntimeOptions.Data.Storage);

        if (!string.IsNullOrEmpty(RuntimeOptions.Data.ConnectionString))
            variables.Add("Nuages__PubSub__Data__ConnectionString", RuntimeOptions.Data.ConnectionString);

        if (RuntimeOptions.ExternalAuth.Enabled)
        {
            if (string.IsNullOrEmpty(RuntimeOptions.ExternalAuth.ValidAudiences))
                throw new Exception("ValidAudiences must be provided when ExternalAuth si Enabled");

            if (string.IsNullOrEmpty(RuntimeOptions.ExternalAuth.ValidIssuers))
                throw new Exception("ValidIssuers must be provided when ExternalAuth si Enabled");

            if (string.IsNullOrEmpty(RuntimeOptions.ExternalAuth.JsonWebKeySetUrlPath))
                throw new Exception("JsonWebKeySetUrlPath must be provided when ExternalAuth si Enabled");

            variables.Add("Nuages__PubSub__ExternalAuth__Enabled", "true");
            variables.Add("Nuages__PubSub__ExternalAuth__ValidAudiences", RuntimeOptions.ExternalAuth.ValidAudiences);
            variables.Add("Nuages__PubSub__ExternalAuth__ValidIssuers", RuntimeOptions.ExternalAuth.ValidIssuers);

            if (!string.IsNullOrEmpty(RuntimeOptions.ExternalAuth.JsonWebKeySetUrlPath))
                variables.Add("Nuages__PubSub__ExternalAuth__JsonWebKeySetUrlPath",
                    RuntimeOptions.ExternalAuth.JsonWebKeySetUrlPath);

            variables.Add("Nuages__PubSub__ExternalAuth__DisableSslCheck",
                RuntimeOptions.ExternalAuth.DisableSslCheck.ToString().ToLower());

            if (!string.IsNullOrEmpty(RuntimeOptions.ExternalAuth.Roles))
                variables.Add("Nuages__PubSub__ExternalAuth__Roles", RuntimeOptions.ExternalAuth.Roles);
        }
    }

    protected  Role CreateWebSocketRole()
    {
        var role = new Role(this, NuagesPubSubRole, new RoleProps
        {
            AssumedBy = new ServicePrincipal("lambda.amazonaws.com")
        });

        role.AddManagedPolicy(CreateLambdaBasicExecutionRolePolicy());
        role.AddManagedPolicy(CreateExecuteApiConnectionRolePolicy());
        role.AddManagedPolicy(CreateDynamoDbRolePolicy());
        role.AddManagedPolicy(CreateSystemsManagerPolicy());
        role.AddManagedPolicy(CreateSecretsManagerPolicy());
        
        return role;
    }

    protected ManagedPolicy CreateDynamoDbRolePolicy(string suffix = "")
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
                        Resources = new[] { $"arn:aws:dynamodb:*:*:table/{StackName}*" }
                    })
                }
            })
        });
    }

    protected ManagedPolicy CreateExecuteApiConnectionRolePolicy(string suffix = "")
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
                        Resources = new[] { $"arn:aws:execute-api:*:*:{ApiRef}/*/*/@connections/*" }
                    })
                }
            })
        });
    }

    protected ManagedPolicy CreateSystemsManagerPolicy(string suffix = "")
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
                        Actions = new[] { "ssm:GetParametersByPath", "appconfig:StartConfigurationSession",
                            "appconfig:GetLatestConfiguration" },
                        Resources = new[] { "*" }
                    })
                }
            })
        });
    }
    
    protected ManagedPolicy CreateSecretsManagerPolicy(string suffix = "")
    {
        return new ManagedPolicy(this, MakeId("SecretsManagerRole" + suffix), new ManagedPolicyProps
        {
            Document = new PolicyDocument(new PolicyDocumentProps
            {
                Statements = new[]
                {
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Effect = Effect.ALLOW,
                        Actions = new[] {  "secretsmanager:GetSecretValue" },
                        Resources = new[] { "*" }
                    })
                }
            })
        });
    }

    protected ManagedPolicy CreateLambdaBasicExecutionRolePolicy(string suffix = "")
    {
        var permissions = new List<string>
        {
            "logs:CreateLogGroup",
            "logs:CreateLogStream",
            "logs:PutLogEvents"
        };

        if (!string.IsNullOrEmpty(ConfigOptions.VpcId))
        {
            permissions.AddRange(new List<string>
            {
                "ec2:DescribeNetworkInterfaces",
                "ec2:CreateNetworkInterface",
                "ec2:DeleteNetworkInterface",
                "ec2:DescribeInstances",
                "ec2:AttachNetworkInterface"
            });
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
                        Actions = permissions.ToArray(),
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

    protected void CreateS3RecordSet(string domainName, CfnDomainName apiGatewayDomainName)
    {
        Console.WriteLine($"CreateS3RecordSet domainName = {domainName}");
        
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

    protected CfnDomainName CreateApiGatewayDomainName(string certficateArn, string domainName)
    {
        Console.WriteLine($"CreateApiGatewayDomainName certficateArn = {certficateArn}");
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

}
