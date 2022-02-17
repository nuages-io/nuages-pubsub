using System.Net;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Options;
using Nuages.PubSub.Services;

// ReSharper disable UnusedType.Global

namespace Nuages.PubSub.Storage.DynamoDb;

public class DynamoDbIndexCreator : BackgroundService
{
    private AmazonDynamoDBClient _client;
    private readonly PubSubOptions _options;

    public DynamoDbIndexCreator(IOptions<PubSubOptions> options)
    {
        _client = new AmazonDynamoDBClient();
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await CreatePubSubConnectionIndexes(stoppingToken);
    }

    private async Task CreatePubSubConnectionIndexes(CancellationToken cancellationToken)
    {
        var table = await _client.DescribeTableAsync($"{_options.StackName}_pub_sub_connection", cancellationToken);
        Console.WriteLine(table.Table.TableStatus.Value);
        if (table.Table.GlobalSecondaryIndexes.Count == 0)
        {
            var response = await _client.UpdateTableAsync(new UpdateTableRequest
            {
                TableName = table.Table.TableName,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new()
                    {
                        AttributeName = "HubAndConnectionId",
                        AttributeType = "S"
                    }
                },
                GlobalSecondaryIndexUpdates = new List<GlobalSecondaryIndexUpdate>
                {
                    new()
                    {
                        Create = new CreateGlobalSecondaryIndexAction
                        {
                            IndexName = "HubAndConnectionId",
                            Projection = new Projection
                            {
                                ProjectionType = ProjectionType.ALL
                            },
                            KeySchema = new List<KeySchemaElement>
                            {
                                new()
                                {
                                    AttributeName = "HubAndConnectionId",
                                    KeyType = "HASH"
                                    
                                }
                            }
                        }
                    }
                }
            }, cancellationToken);

            await WaitForActiveTable(table.Table.TableName, cancellationToken);
           
            response = await _client.UpdateTableAsync(new UpdateTableRequest
            {
                TableName = table.Table.TableName,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new()
                    {
                        AttributeName = "HubAndUserId",
                        AttributeType = "S"
                    }
                },
                GlobalSecondaryIndexUpdates = new List<GlobalSecondaryIndexUpdate>
                {
                    new()
                    {
                        Create = new CreateGlobalSecondaryIndexAction
                        {
                            IndexName = "HubAndUserId",
                            Projection = new Projection
                            {
                                ProjectionType = ProjectionType.ALL
                            },
                            KeySchema = new List<KeySchemaElement>
                            {
                                new()
                                {
                                    AttributeName = "HubAndUserId",
                                    KeyType = "HASH"
                                }
                            }
                    
                        }
                    }
                }
            }, cancellationToken);
            
            var res = response.HttpStatusCode == HttpStatusCode.Accepted;
        }
        
        // pubSubConnection.AddGlobalSecondaryIndex(new GlobalSecondaryIndexProps
        // {
        //     IndexName = "HubAndConnectionId",
        //     ProjectionType = ProjectionType.ALL,
        //     PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
        //     {
        //         Name = "HubAndConnectionId",
        //         Type = AttributeType.STRING
        //     }
        // });
        //
        // pubSubConnection.AddGlobalSecondaryIndex(new GlobalSecondaryIndexProps
        // {
        //     IndexName = "HubAndUserId",
        //     ProjectionType = ProjectionType.ALL,
        //     PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
        //     {
        //         Name = "HubAndUserId",
        //         Type = AttributeType.STRING
        //     }
        // });
    }

    private async Task WaitForActiveTable(string tableName, CancellationToken cancellationToken)
    {
        var table = await _client.DescribeTableAsync($"{_options.StackName}_pub_sub_connection", cancellationToken);
        
        while (table.Table.TableStatus != TableStatus.ACTIVE)
        {
            Console.WriteLine(table.Table.TableStatus.Value);
            Thread.Sleep(10000);
            table = await _client.DescribeTableAsync($"{_options.StackName}_pub_sub_connection", cancellationToken);
        }
    }
}