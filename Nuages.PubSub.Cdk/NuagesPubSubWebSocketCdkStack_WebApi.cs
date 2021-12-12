using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.Apigatewayv2;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.Route53;
using Amazon.CDK.AWS.SAM;
using Amazon.JSII.Runtime.Deputy;
using Constructs;
using CfnDomainName = Amazon.CDK.AWS.Apigatewayv2.CfnDomainName;
using CfnDomainNameProps = Amazon.CDK.AWS.Apigatewayv2.CfnDomainNameProps;

// ReSharper disable VirtualMemberNeverOverridden.Global

namespace Nuages.PubSub.Cdk;

public partial class NuagesPubSubWebSocketCdkStack<T>
{
    protected string? WebApiAsset { get; set; }
    
    private void CreateWebApi(string url)
    {
        var role = CreateWebApiRole();

        if (string.IsNullOrEmpty(WebApiAsset))
        {
            throw new Exception("WebApiAsset must be assigned");
        }

        
        
        // ReSharper disable once UnusedVariable
        var func = new Function(this, "AspNetCoreFunction", new FunctionProps
        {
            FunctionName = MakeId("AspNetCoreFunction"),
            Code = Code.FromAsset(WebApiAsset),
            Handler = "Nuages.PubSub.Samples.API::Nuages.PubSub.Samples.API.LambdaEntryPoint::FunctionHandlerAsync",
            Runtime = Runtime.DOTNET_CORE_3_1,
            Role = role,
            Timeout = Duration.Seconds(30),
            Environment = new Dictionary<string, string>
            {
                {"Nuages__PubSub__Uri", url},
                {"Nuages__PubSub__Region", Aws.REGION},
                {"Nuages__PubSub__TableNamePrefix", TableNamePrefix ?? "" },
                {"Nuages__PubSub__StackName", StackName ?? "" }
            },
            Tracing = Tracing.ACTIVE
        });
        
        func.AddEventSource(new ApiEventSource("ANY", "/{proxy+}", new MethodOptions
        {
            ApiKeyRequired = true
        }));
        
        func.AddEventSource(new ApiEventSource("ANY", "/", new MethodOptions
        {
            ApiKeyRequired = true
        }));
        
        
        var useCustomDomain = Convert.ToBoolean(Node.TryGetContext(ContextUseCustomDomainName));

        if (useCustomDomain)
        {
            var domainName = (string) Node.TryGetContext(ContextDomainNameApi);
            var certficateArn = (string) Node.TryGetContext(ContextCertificateArn);
        
            var apiGatewayDomainName = new CfnDomainName(this, "NuagesApiDomainName", new CfnDomainNameProps
            {
                DomainName = domainName,
                DomainNameConfigurations = new [] { new CfnDomainName.DomainNameConfigurationProperty
                {
                    EndpointType = "REGIONAL",
                    CertificateArn = certficateArn
                }}
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

            var webApi = (RestApi) Node.Children.Single(c => c.GetType() == typeof(RestApi));

           
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

            
            // ReSharper disable once UnusedVariable
            // var overrides = new CfnApiGatewayManagedOverrides(this, MakeId("ApiGatewayOverriudes"),
            //     new CfnApiGatewayManagedOverridesProps
            //     {
            //         ApiId = webApi.RestApiId,
            //         Stage = new CfnApiGatewayManagedOverrides.StageOverridesProperty
            //         {
            //             DefaultRouteSettings =  new CfnApiGatewayManagedOverrides.RouteSettingsProperty
            //             {
            //                 DataTraceEnabled = true
            //             }
            //         }
            //     });
            
            
            // ReSharper disable once UnusedVariable
            var apiKey = new ApiKey(this, "WebApiKey");

            usagePlan.AddApiKey(apiKey);
            
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
        
        
    }

    private void PrintNode(Node deploymentStageNode)
    {
       
        Console.WriteLine(deploymentStageNode.Id + " " + deploymentStageNode.GetType());

        
        foreach (var c in deploymentStageNode.Children)
        {
            PrintNode(c.Node);
        }
    }

    protected virtual Role CreateWebApiRole()
    {
        var role = new Role(this, "RoleWebApi", new RoleProps
        {
            AssumedBy = new ServicePrincipal("lambda.amazonaws.com")
        });

        role.AddManagedPolicy(CreateLambdaBasicExecutionRolePolicy("API"));
        
        role.AddManagedPolicy(CreateLambdaFullAccessRolePolicy());
            
        role.AddManagedPolicy(CreateDynamoDbWebApiRolePolicy());
        
        role.AddManagedPolicy(CreateExecuteApiConnectionWebApiRolePolicy());
        
        return role;

    }

    protected virtual ManagedPolicy CreateExecuteApiConnectionWebApiRolePolicy()
    {
        return new ManagedPolicy(this, MakeId("ExecuteApiConnectionRoleWebApi"), new ManagedPolicyProps
        {
            Document = new PolicyDocument(new PolicyDocumentProps
            {
                Statements = new []{ new PolicyStatement(new PolicyStatementProps
                {
                    Effect = Effect.ALLOW,
                    Actions = new []{"execute-api:ManageConnections"},
                    Resources = new []{"arn:aws:execute-api:*:*:*/@connections/*"}
                })}
            })
            //ManagedPolicyName = MakeId("ExecuteApiConnectionRoleWebApi")
        });
    }
    
    private IManagedPolicy CreateDynamoDbWebApiRolePolicy()
    {
        var id = MakeId("DynamoDbRoleWebApi");
        
        return new ManagedPolicy(this, id, new ManagedPolicyProps
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
     private IManagedPolicy CreateLambdaFullAccessRolePolicy()
    {
        return new ManagedPolicy(this, MakeId("LambdaFullAccessRole"), new ManagedPolicyProps
        {
            Document = new PolicyDocument(new PolicyDocumentProps
            {
                Statements = new []{ new PolicyStatement(new PolicyStatementProps
                {
                    Effect = Effect.ALLOW,
                    Actions = new []{"cloudformation:DescribeStacks",
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
                        "xray:BatchGetTraces"},
                    Resources = new []{"*"}
                }),
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Effect = Effect.ALLOW,
                        Actions = new []{"iam:PassRole"},
                        Resources = new []{"*"},
                        Conditions = new Dictionary<string, object>
                        {
                            { 
                                "StringEquals",  new Dictionary<string, string>
                                {
                                    { "iam:PassedToService", "lambda.amazonaws.com" }
                                }
                                
                            }
                           
                        }
                    }),
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Effect = Effect.ALLOW,
                        Actions = new []{"logs:DescribeLogStreams",
                            "logs:GetLogEvents",
                            "logs:FilterLogEvents"},
                        Resources = new []{"arn:aws:logs:*:*:log-group:/aws/lambda/*"}
                    })
                }
                
            })
            //ManagedPolicyName = MakeId("LambdaFullAccessRole")
        });
    }
}