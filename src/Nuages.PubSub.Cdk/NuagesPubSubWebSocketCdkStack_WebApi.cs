using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.Apigatewayv2;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.Route53;
using CfnDomainName = Amazon.CDK.AWS.Apigatewayv2.CfnDomainName;
using CfnDomainNameProps = Amazon.CDK.AWS.Apigatewayv2.CfnDomainNameProps;

// ReSharper disable VirtualMemberNeverOverridden.Global

namespace Nuages.PubSub.Cdk;

public partial class NuagesPubSubWebSocketCdkStack<T>
{
    protected string? ApiAsset { get; set; }

    private void CreateWebApi(string url)
    {
        var role = CreateWebApiRole();

        var func = CreateWebApiFunction(url, role);

        AddEventSources(func);

        var webApi = (RestApi)Node.Children.Single(c => c.GetType() == typeof(RestApi));
        
        var domainName = (string)Node.TryGetContext(ContextDomainNameApi);

        if (!string.IsNullOrEmpty(domainName))
        {
            var certficateArn = (string)Node.TryGetContext(ContextCertificateArnApi);

            Console.WriteLine($"ContextCertificateArnApi = {ContextCertificateArnApi}");
            var apiGatewayDomainName = new CfnDomainName(this, "NuagesApiDomainName", new CfnDomainNameProps
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

            var hostedZone = HostedZone.FromLookup(this, "LookupApi", new HostedZoneProviderProps
            {
                DomainName = GetBaseDomain(domainName)
            });

            // ReSharper disable once UnusedVariable
            var recordSet = new CfnRecordSet(this, "Route53RecordSetGroupApi", new CfnRecordSetProps
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

            // ReSharper disable once UnusedVariable
            var apiMapping = new CfnApiMapping(this, MakeId("NuagesRestApiMapping"), new CfnApiMappingProps
            {
                DomainName = apiGatewayDomainName.DomainName,
                ApiId = webApi.RestApiId,
                Stage = webApi.DeploymentStage.StageName
            });

            // ReSharper disable once UnusedVariable
            var output = new CfnOutput(this, "NuagesPubSubApi", new CfnOutputProps
            {
                Value = $"https://{apiGatewayDomainName.DomainName}",
                Description = "Custom Url for the Web API"
            });
        }

        // ReSharper disable once UnusedVariable
        var usagePlan = new UsagePlan(this, MakeId("WebApiUsagePlan"), new UsagePlanProps
        {
            ApiStages = new IUsagePlanPerApiStage[]
            {
                new UsagePlanPerApiStage
                {
                    Api = webApi,
                    Stage = webApi.DeploymentStage
                }
            }
        });

        var key = (string)Node.TryGetContext(ContextApiKeyApi);
        
        // ReSharper disable once UnusedVariable
        var apiKey = new ApiKey(this, MakeId("WebApiKey"), new ApiKeyProps
        {
            Value = key
        });

        usagePlan.AddApiKey(apiKey);
    }

    protected virtual void AddEventSources(Function func)
    {
        func.AddEventSource(new ApiEventSource("ANY", "/{proxy+}", new MethodOptions
        {
            ApiKeyRequired = true
        }));

        func.AddEventSource(new ApiEventSource("ANY", "/", new MethodOptions
        {
            ApiKeyRequired = true
        }));

        func.AddEventSource(new ApiEventSource("ANY", "/swagger/{proxy+}", new MethodOptions
        {
            ApiKeyRequired = false
        }));

        func.AddEventSource(new ApiEventSource("ANY", "/swagger", new MethodOptions
        {
            ApiKeyRequired = false
        }));
    }

    private ISecurityGroup? _vpcApiSecurityGroup;
    
    private ISecurityGroup[]? CurrentVpcApiSecurityGroup
    {
        get
        {
            if (!string.IsNullOrEmpty(VpcId))
            {
                _vpcApiSecurityGroup ??= CreateVpcApiSecurityGroup();
            }

            return _vpcApiSecurityGroup != null ? new[] { _vpcApiSecurityGroup } : null;
        }
    }

    protected virtual SecurityGroup CreateVpcApiSecurityGroup()
    {
        return new SecurityGroup(this, MakeId("ApiSecurityGroup"), new SecurityGroupProps
        {
            Vpc = CurrentVpc!,
            AllowAllOutbound = true,
            Description = "PuSub API Security Group"
        });
    }

    protected virtual Function CreateWebApiFunction(string url, Role role)
    {
        if (string.IsNullOrEmpty(ApiAsset))
        {
            throw new Exception("WebApiAsset must be assigned");
        }
        
        var func = new Function(this, "API", new FunctionProps
        {
            FunctionName = MakeId("API"),
            Code = Code.FromAsset(ApiAsset),
            Handler = WebApiHandler,
            Runtime = Runtime.DOTNET_CORE_3_1,
            Role = role,
            Timeout = Duration.Seconds(30),
            MemorySize = 512,
            Environment = new Dictionary<string, string>
            {
                { "Nuages__PubSub__Uri", url },
                { "Nuages__PubSub__Region", Aws.REGION },
                { "Nuages__PubSub__StackName", StackName },
                { "Nuages__PubSub__Issuer", Issuer ?? "" },
                { "Nuages__PubSub__Audience", Audience ?? "" },
                { "Nuages__PubSub__Secret", Secret ?? "" }
            },
            Tracing = Tracing.ACTIVE,
            Vpc = CurrentVpc,
            AllowPublicSubnet = true,
            SecurityGroups = CurrentVpcApiSecurityGroup
        });
        
        if (Proxy != null)
        {
            Proxy.GrantConnect(func, ProxyUser);
            
            if (CurrentVpcApiSecurityGroup != null)
            {
                ProxySg.AddIngressRule(CurrentVpcApiSecurityGroup.First(), Port.Tcp(3306), "PubSub API MySql");
            }
        }
        
        return func;
    }

    protected virtual Role CreateWebApiRole()
    {
        var role = new Role(this, "RoleWebApi", new RoleProps
        {
            AssumedBy = new ServicePrincipal("lambda.amazonaws.com")
        });

        role.AddManagedPolicy(CreateLambdaBasicExecutionRolePolicy("API"));
        role.AddManagedPolicy(CreateLambdaFullAccessRolePolicy());
        role.AddManagedPolicy(CreateDynamoDbRolePolicy("API"));
        role.AddManagedPolicy(CreateSystemsManagerParametersRolePolicy("API"));
        role.AddManagedPolicy(CreateExecuteApiConnectionRolePolicy("API"));

        return role;
    }

    private IManagedPolicy CreateLambdaFullAccessRolePolicy()
    {
        return new ManagedPolicy(this, MakeId("LambdaFullAccessRole"), new ManagedPolicyProps
        {
            Document = new PolicyDocument(new PolicyDocumentProps
            {
                Statements = new[]
                {
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Effect = Effect.ALLOW,
                        Actions = new[]
                        {
                            "cloudformation:DescribeStacks",
                            "cloudformation:ListStackResources",
                            "cloudwatch:ListMetrics",
                            "cloudwatch:GetMetricData",
                            "ec2:DescribeSecurityGroups",
                            "ec2:DescribeSubnets",
                            "ec2:DescribeVpcs",
                            "kms:ListAliases",
                            "iam:GetPolicy",
                            "iam:GetPolicyVersion",
                            "iam:GetRole",
                            "iam:GetRolePolicy",
                            "iam:ListAttachedRolePolicies",
                            "iam:ListRolePolicies",
                            "iam:ListRoles",
                            "lambda:*",
                            "logs:DescribeLogGroups",
                            "states:DescribeStateMachine",
                            "states:ListStateMachines",
                            "tag:GetResources",
                            "xray:GetTraceSummaries",
                            "xray:BatchGetTraces"
                        },
                        Resources = new[] { "*" }
                    }),
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Effect = Effect.ALLOW,
                        Actions = new[] { "iam:PassRole" },
                        Resources = new[] { "*" },
                        Conditions = new Dictionary<string, object>
                        {
                            {
                                "StringEquals", new Dictionary<string, string>
                                {
                                    { "iam:PassedToService", "lambda.amazonaws.com" }
                                }
                            }
                        }
                    }),
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Effect = Effect.ALLOW,
                        Actions = new[]
                        {
                            "logs:DescribeLogStreams",
                            "logs:GetLogEvents",
                            "logs:FilterLogEvents"
                        },
                        Resources = new[] { "arn:aws:logs:*:*:log-group:/aws/lambda/*" }
                    })
                }
            })
        });
    }
}