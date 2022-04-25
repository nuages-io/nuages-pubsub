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

public partial class PubSubWebSocketCdkStack<T>
{
    protected string? ApiAsset { get; set; }

    private void CreateWebApi(string url)
    {
        var role = CreateWebApiRole();

        var func = CreateWebApiFunction(url, role);

        AddEventSources(func);

        var webApi = (RestApi)Node.Children.Single(c => c.GetType() == typeof(RestApi));
        
        if (!string.IsNullOrEmpty(ApiDomainName))
        {
            var apiGatewayDomainName = new CfnDomainName(this, "NuagesApiDomainName", new CfnDomainNameProps
            {
                DomainName = ApiDomainName,
                DomainNameConfigurations = new[]
                {
                    new CfnDomainName.DomainNameConfigurationProperty
                    {
                        EndpointType = "REGIONAL",
                        CertificateArn = ApiCertificateArn
                    }
                }
            });

            var hostedZone = HostedZone.FromLookup(this, "LookupApi", new HostedZoneProviderProps
            {
                DomainName = GetBaseDomain(ApiDomainName)
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
                Name = ApiDomainName,
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

        // ReSharper disable once UnusedVariable
        var apiKey = new ApiKey(this, MakeId("WebApiKey"), new ApiKeyProps
        {
            Value = ApiApiKey
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
        
        func.AddEventSource(new ApiEventSource("ANY", "/health/{proxy+}", new MethodOptions
        {
            ApiKeyRequired = false
        }));

        func.AddEventSource(new ApiEventSource("ANY", "/health", new MethodOptions
        {
            ApiKeyRequired = false
        }));
    }

    private ISecurityGroup? _vpcApiSecurityGroup;
    
    private ISecurityGroup[] VpcApiSecurityGroups
    {
        get
        {
            if (_vpcApiSecurityGroup == null && !string.IsNullOrEmpty(VpcId))
            {
                _vpcApiSecurityGroup ??= CreateVpcApiSecurityGroup();
            }
            
            var list = new List<ISecurityGroup>();
            
            if (_vpcApiSecurityGroup != null)
                list.Add(_vpcApiSecurityGroup);

            if (SecurityGroup != null)
                list.Add(SecurityGroup);

            return list.ToArray();
        }
    }

    protected virtual SecurityGroup CreateVpcApiSecurityGroup()
    {
        Console.WriteLine("CreateVpcApiSecurityGroup");
        
        return new SecurityGroup(this, MakeId("ApiSecurityGroup"), new SecurityGroupProps
        {
            Vpc = CurrentVpc!,
            AllowAllOutbound = true,
            Description = "PubSub API Security Group"
        });
    }

    protected virtual Function CreateWebApiFunction(string url, Role role)
    {
        Console.WriteLine($"CreateWebApiFunction url = {url}");
        
        if (string.IsNullOrEmpty(ApiAsset))
        {
            throw new Exception("WebApiAsset must be assigned");
        }
        
        var func = new Function(this, "API", new FunctionProps
        {
            FunctionName = MakeId("API"),
            Code = Code.FromAsset(ApiAsset),
            Handler = WebApiHandler,
            Runtime = new Runtime("dotnet6"),
            Role = role,
            Timeout = Duration.Seconds(30),
            MemorySize = 512,
            Environment = GetWebApiEnvVariables(url),
            Tracing = Tracing.ACTIVE,
            Vpc = CurrentVpc,
            AllowPublicSubnet = true,
            SecurityGroups = VpcApiSecurityGroups
        });

        Proxy?.GrantConnect(func, DatabaseProxyUser);
        
        return func;
    }

    // ReSharper disable once UnusedParameter.Global
    protected virtual void AddWebApiEnvironmentVariables(Dictionary<string, string> environmentVariables)
    {
    }
    
    private Dictionary<string, string> GetWebApiEnvVariables(string url)
    {
        var variables = new Dictionary<string, string>
        {
            { "Nuages__PubSub__Uri", url },
            { "Nuages__PubSub__Region", Aws.REGION },
            { "Nuages__PubSub__StackName", StackName }
        };

        if (!string.IsNullOrEmpty(Auth_Audience))
            variables.Add("Nuages__PubSub__Auth__Audience",Auth_Audience);
        
        if (!string.IsNullOrEmpty(Auth_Issuer))
            variables.Add("Nuages__PubSub__Auth__Issuer", Auth_Issuer);
        
        if (!string.IsNullOrEmpty(Auth_Secret))
            variables.Add("Nuages__PubSub__Auth__Secret",Auth_Secret);
        
        if (!string.IsNullOrEmpty(DataStorage))
            variables.Add("Nuages__PubSub__Data__Storage",DataStorage);
        
        if (!string.IsNullOrEmpty(DataConnectionString))
            variables.Add("Nuages__PubSub__Data__ConnectionString", DataConnectionString);
        
        
        AddWebApiEnvironmentVariables(variables);
        
        return variables;
    }

    protected virtual Role CreateWebApiRole()
    {
        var role = new Role(this, "RoleWebApi", new RoleProps
        {
            AssumedBy = new ServicePrincipal("lambda.amazonaws.com")
        });

        role.AddManagedPolicy(CreateLambdaBasicExecutionRolePolicy("API"));
        role.AddManagedPolicy(CreateDynamoDbRolePolicy("API"));
        role.AddManagedPolicy(CreateSystemsManagerPolicy("API"));
        role.AddManagedPolicy(CreateExecuteApiConnectionRolePolicy("API"));
        role.AddManagedPolicy(CreateSecretsManagerPolicy("API"));

        return role;
    }
}