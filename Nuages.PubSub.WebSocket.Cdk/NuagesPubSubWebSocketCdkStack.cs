using System;
using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.CertificateManager;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Route53;
using Amazon.CDK.AWS.Route53.Targets;
using Constructs;

// ReSharper disable ObjectCreationAsStatement

namespace Nuages.PubSub.WebSocket.Cdk
{
    public class NuagesPubSubWebSocketCdkStack : Stack
    {
        public  virtual void BuildTheThing()
        {
            var domainName = (string) Node.TryGetContext("Nuages/PubSub/DomainName");
            var certficateArn = (string) Node.TryGetContext("Nuages/PubSub/CertificateArn");
            var hostedZoneId = (string) Node.TryGetContext("Nuages/PubSub/HostedZoneId");

            
            Console.WriteLine($"HostedZoneUId = {hostedZoneId}");
            var apiGatewayDomainName = GetApiGatewayDomainName(certficateArn, domainName);

            var s3Recordset = GetS3RecordSet(hostedZoneId, domainName, apiGatewayDomainName);

            //Role
            var role = GetRole();
            
            //Functions
            //Api
            //Authorizer
            //Routes + Integrations
            //Deployment
            //Stage
            //Permissions
            //Api Mapping
            //Outputs
        }
        
        internal NuagesPubSubWebSocketCdkStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            
        }

        private Role GetRole()
        {
            var role = new Role(this, "NuagesPuSubRole", new RoleProps
            {
                AssumedBy = new ServicePrincipal("lambda.amazonaws.com"),
                RoleName = "NuagesPubSubRole"
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

        public static string GetBaseDomain(string domainName)
        {
            var tokens = domainName.Split('.');

            // only split 3 segments like www.west-wind.com
            if (tokens == null || tokens.Length != 3)
                return domainName;

            var tok  = new List<string>(tokens);
            var remove = tokens.Length - 2;
            tok.RemoveRange(0, remove);

            return tok[0] + "." + tok[1]; ;                                
        }
        
        private RecordSet GetS3RecordSet(string hostedZoneId, string domainName, DomainName_ apiGatewayDomainName)
        {
            var hostedZone = HostedZone.FromLookup(this, "Lookup", new HostedZoneProviderProps
            {
                DomainName = GetBaseDomain(domainName)
            });

            return new RecordSet(this, "Route53RecordSetGroup", new RecordSetProps
            {
                RecordName = domainName,
                RecordType = RecordType.A,
                Zone = hostedZone,
                Target = RecordTarget.FromAlias(new ApiGatewayDomain(apiGatewayDomainName))
            });
        }

        private DomainName_ GetApiGatewayDomainName(string certficateArn, string domainName)
        {
            var cert = Certificate.FromCertificateArn(this, "NusagePubSubCert", certficateArn);
            var apiGatewayDomainName = new DomainName_(this, "NuagesDomainName", new DomainNameProps
            {
                DomainName = domainName,
                EndpointType = EndpointType.REGIONAL,
                Certificate = cert
            });
            return apiGatewayDomainName;
        }
    }
}
