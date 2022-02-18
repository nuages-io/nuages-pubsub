using System.Net;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Options;
using Nuages.PubSub.Services;
using Nuages.PubSub.Storage.DynamoDb.DataModel;
// ReSharper disable RedundantAssignment
// ReSharper disable EntityNameCapturedOnly.Local

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

    async Task WaitForTablesAndIndexes(CancellationToken cancellationToken)
    {
        var tables = await DescribeTables(cancellationToken);

        while (tables.Any(t => t.TableStatus != TableStatus.ACTIVE) ||
               tables.Any(t => t.GlobalSecondaryIndexes.Any(i => i.IndexStatus != IndexStatus.ACTIVE)))
        {
            Thread.Sleep(10000);
            Console.WriteLine("Updating...");
            tables = await DescribeTables(cancellationToken);
            
        }
    }

    private async Task<List<TableDescription>> DescribeTables(CancellationToken cancellationToken)
    {
        var tables = new List<TableDescription>();
        var tableConnection =
            await _client.DescribeTableAsync($"{_options.StackName}_pub_sub_connection", cancellationToken);
        var tableAck = await _client.DescribeTableAsync($"{_options.StackName}_pub_sub_ack", cancellationToken);
        var tableGroupConnection =
            await _client.DescribeTableAsync($"{_options.StackName}_pub_sub_group_connection", cancellationToken);
        var tableGroupUser =
            await _client.DescribeTableAsync($"{_options.StackName}_pub_sub_group_user", cancellationToken);

        tables.Add(tableConnection.Table);
        tables.Add(tableAck.Table);
        tables.Add(tableGroupConnection.Table);
        tables.Add(tableGroupUser.Table);
        return tables;
    }

    private async Task CreatePubSubConnectionIndexes(CancellationToken cancellationToken)
    {
        var connectionTable = $"{_options.StackName}_pub_sub_connection";
        var connection = new PubSubConnection();
        //await CreateIndex(connectionTable, nameof(connection.HubAndConnectionId), cancellationToken);
        //await CreateIndex(connectionTable, nameof(connection.HubAndUserId), cancellationToken);
        
        // var ackTable = $"{_options.StackName}_pub_sub_ack";
        // var ack = new PubSubAck();
        // await CreateIndex(ackTable, nameof(ack.HubAndConnectionIdAndAckId), cancellationToken);
        //
        var groupTable = $"{_options.StackName}_pub_sub_group_connection";
        var group = new PubSubGroupConnection();
        // await CreateIndex(groupTable, nameof(group.HubAndGroup), cancellationToken);
        // await CreateIndex(groupTable, nameof(group.HubAndGroupAndConnectionId), cancellationToken);
        // await CreateIndex(groupTable, nameof(group.HubAndConnectionId), cancellationToken);
        // await CreateIndex(groupTable, nameof(group.HubAndUserId), cancellationToken);
        // await CreateIndex(groupTable, nameof(group.HubAndGroupAndUserId), cancellationToken);
        
        var userTable = $"{_options.StackName}_pub_sub_group_user";
        var user = new PubSubGroupUser();
        // await CreateIndex(userTable, nameof(user.HubAndUserId), cancellationToken);
        // await CreateIndex(userTable, nameof(user.HubAndGroupAndUserId), cancellationToken);
    }

    private async Task<UpdateTableResponse?> CreateIndex(string tableName, string attributeName,
        CancellationToken cancellationToken, bool retry = true)
    {
        await WaitForTablesAndIndexes(cancellationToken);
        
        var indexName = $"{attributeName}_Index";
        var response = await _client.DescribeTableAsync(tableName, cancellationToken);

        if (response.Table.GlobalSecondaryIndexes.Exists(i => i.IndexName == indexName))
            return null;
        
        Console.WriteLine($"Creating index on table {tableName} {indexName}");

        try
        {
            return  await _client.UpdateTableAsync(new UpdateTableRequest
            {
                TableName = tableName,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new()
                    {
                        AttributeName = attributeName,
                        AttributeType = "S"
                    }
                },
                GlobalSecondaryIndexUpdates = new List<GlobalSecondaryIndexUpdate>
                {
                    new()
                    {
                        Create = new CreateGlobalSecondaryIndexAction
                        {
                            IndexName = indexName,
                            Projection = new Projection
                            {
                                ProjectionType = ProjectionType.ALL
                            },
                            KeySchema = new List<KeySchemaElement>
                            {
                                new()
                                {
                                    AttributeName = attributeName,
                                    KeyType = "HASH"
                                }
                            }
                        }
                    }
                }
            }, cancellationToken);
            
        }
        catch (LimitExceededException)
        {
            if (retry)
            {
                Thread.Sleep(10000);
                return await CreateIndex(tableName, attributeName, cancellationToken, false);
            }
            
            throw;
        }
    }
}