using System.Diagnostics.CodeAnalysis;
using Amazon.CDK;
using Amazon.CDK.AWS.Chatbot;
using Amazon.CDK.AWS.CodeBuild;
using Amazon.CDK.AWS.CodePipeline.Actions;
using Amazon.CDK.AWS.CodeStarNotifications;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.SNS;
using Amazon.CDK.Pipelines;
using Constructs;
using Microsoft.Extensions.Configuration;

// ReSharper disable ObjectCreationAsStatement

namespace Nuages.PubSub.Cdk.Deploy;

[SuppressMessage("Performance", "CA1806:Do not ignore method results")]
public class PubSubStackWithPipeline : Stack
{
    public static void Create(Construct scope, ConfigOptions options)
    {
        // ReSharper disable once ObjectCreationAsStatement
        new PubSubStackWithPipeline(scope, $"{options.StackName}-PipelineStack", options, new StackProps
        {
            Env = new Amazon.CDK.Environment
            {
                Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION")
            }
        });
    }

    private PubSubStackWithPipeline(Construct scope, string id, ConfigOptions options, IStackProps props) : base(scope, id, props)
    {
        var pipeline = new CodePipeline(this, "pipeline", new CodePipelineProps
        {
            PipelineName = $"{options.StackName}-Pipeline",
            SynthCodeBuildDefaults = new CodeBuildOptions
            {
                RolePolicy = new PolicyStatement[]
                {
                    new (new PolicyStatementProps
                    {
                        Effect = Effect.ALLOW,
                        Actions = new[]
                        {
                            "route53:*"
                        },
                        Resources = new[] { "*" }
                    }),
                    new (new PolicyStatementProps
                    {
                        Effect = Effect.ALLOW,
                        Actions = new[] { "ssm:GetParametersByPath", "appconfig:GetConfiguration" },
                        Resources = new[] { "*" }
                    })
                }
            },
            Synth = new ShellStep("Synth",
                new ShellStepProps
                {
                    Input = CodePipelineSource.GitHub(options.CDKPipeline!.GitHubRepository!,
                        "main",
                        new GitHubSourceOptions
                        {
                            Authentication = SecretValue.PlainText(options.CDKPipeline!.GithubToken!),
                            Trigger = GitHubTrigger.WEBHOOK
                        }),
                    Commands = new []
                    {
                        "npm install -g aws-cdk",
                        "cdk synth"
                    }
                }),
            CodeBuildDefaults = new CodeBuildOptions
            {
                BuildEnvironment = null,
                PartialBuildSpec = BuildSpec.FromObject(new Dictionary<string, object>
                {
                    {
                        "phases", new Dictionary<string, object>
                        {
                            {
                                "install", new Dictionary<string, object>
                                {
                                    { "commands", new [] { "/usr/local/bin/dotnet-install.sh --channel LTS"} }
                                }
                            }
                        }
                    }
                }),
                RolePolicy = new PolicyStatement[]
                {
                    new (new PolicyStatementProps
                    {
                        Effect = Effect.ALLOW,
                        Actions = new[]
                        {
                            "route53:*"
                        },
                        Resources = new[] { "*" }
                    }),
                    new (new PolicyStatementProps
                    {
                        Effect = Effect.ALLOW,
                        Actions = new[] { "ssm:GetParametersByPath", "appconfig:GetConfiguration" },
                        Resources = new[] { "*" }
                    })
                }
            }
        });
            
        pipeline.AddStage(new PipelineAppStage(this, "Deploy", options, new StageProps
        {
            Env = new Amazon.CDK.Environment
            {
                Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION")
            }
        }));

        pipeline.BuildPipeline();
        
        var arn = options.CDKPipeline!.NotificationTargetArn;

        if (!string.IsNullOrEmpty(arn))
        {
            INotificationRuleTarget? target;
            
            if (arn.StartsWith("arn:aws:chatbot"))
                target = SlackChannelConfiguration.FromSlackChannelConfigurationArn(this, "SlackChannel", arn);
            else
            {
                target = Topic.FromTopicArn(this, "SNSTopic", arn);
            }
            
            new NotificationRule(this, "Notification", new NotificationRuleProps
            {
                Events = new []
                {
                    "codepipeline-pipeline-pipeline-execution-failed",
                    "codepipeline-pipeline-pipeline-execution-canceled",
                    "codepipeline-pipeline-pipeline-execution-started",
                    "codepipeline-pipeline-pipeline-execution-resumed",
                    "codepipeline-pipeline-pipeline-execution-succeeded",
                    "codepipeline-pipeline-pipeline-execution-superseded"
                },
                Source = pipeline.Pipeline,
                Targets = new []
                {
                    target
                },
                DetailType = DetailType.BASIC,
                NotificationRuleName = pipeline.Pipeline.PipelineName
            });
        }

        // new CfnWebhook(this, "gitHubWebHook", new CfnWebhookProps
        // {
        //     Authentication = "GITHUB_HMAC",
        //     AuthenticationConfiguration = new CfnWebhook.WebhookAuthConfigurationProperty
        //     {
        //         SecretToken = configuration["GithubToken"]
        //     },
        //     Filters = new[]
        //     {
        //         new CfnWebhook.WebhookFilterRuleProperty
        //         {
        //             JsonPath = "$.action",
        //
        //             // the properties below are optional
        //             MatchEquals = "published"
        //         }
        //     },
        //     TargetAction = configuration["GithubRepository"].Replace("/", "_"),
        //     TargetPipeline = $"{configuration["StackName"]}-Pipeline",
        //     TargetPipelineVersion = 1,
        //     RegisterWithThirdParty = true
        // });
    }

    private class PipelineAppStage : Stage
    {
        public PipelineAppStage(Construct scope, string id, ConfigOptions options, IStageProps props) : base(scope, id, props)
        {
            PubSubStack.CreateStack(this, options);
        }
    }
}