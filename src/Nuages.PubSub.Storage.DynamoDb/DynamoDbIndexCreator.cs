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

    private Task CreatePubSubConnectionIndexes(CancellationToken cancellationToken)
    {

        var connectionTableName = $"{_options.StackName}_pub_sub_connection";
        var connection = new PubSubConnection();
        CreateIndex(connectionTableName, nameof(connection.HubAndConnectionId), cancellationToken)
            .Wait(cancellationToken);
        CreateIndex(connectionTableName, nameof(connection.HubAndUserId), cancellationToken)
            .Wait(cancellationToken);

        return Task.CompletedTask;
        // var connectionTable = await _client.DescribeTableAsync(connectionTableName, cancellationToken);
        //
        // Console.WriteLine(connectionTableName + " " + connectionTable.Table.TableStatus);
        //
        // while (connectionTable.Table.TableStatus != TableStatus.ACTIVE)
        // {
        //     Thread.Sleep(10000);
        //     connectionTable = await _client.DescribeTableAsync(connectionTable.Table.TableName, cancellationToken);
        //     Console.WriteLine(connectionTable.Table.TableName + " " + connectionTable.Table.TableStatus + " 1");
        // }
        //
        // 
        // await CreateIndex(connectionTableName, nameof(connection.HubAndConnectionId), cancellationToken);
        // Thread.Sleep(10000);
        //
        // connectionTable = await _client.DescribeTableAsync(connectionTable.Table.TableName, cancellationToken);
        // while (connectionTable.Table.TableStatus != TableStatus.ACTIVE)
        // {
        //     Thread.Sleep(10000);
        //     connectionTable = await _client.DescribeTableAsync(connectionTable.Table.TableName, cancellationToken);
        //     Console.WriteLine(connectionTable.Table.TableName + " " + connectionTable.Table.TableStatus  + " 2");
        // }
        // await CreateIndex(connectionTableName, nameof(connection.HubAndUserId), cancellationToken);
        // Thread.Sleep(10000);
        //
        // connectionTable = await _client.DescribeTableAsync(connectionTable.Table.TableName, cancellationToken);
        // while (connectionTable.Table.TableStatus != TableStatus.ACTIVE)
        // {
        //     Thread.Sleep(10000);
        //     connectionTable = await _client.DescribeTableAsync(connectionTable.Table.TableName, cancellationToken);
        //     Console.WriteLine(connectionTable.Table.TableName + " " + connectionTable.Table.TableStatus + " 3");
        // }


        //
        // var ackTable = $"{_options.StackName}_pub_sub_ack";
        // var ack = new PubSubAck();
        // await CreateIndex(ackTable, nameof(ack.HubAndConnectionIdAndAckId), cancellationToken);
        // await WaitForActiveTable(ackTable, cancellationToken);
        //
        // var groupTable = $"{_options.StackName}_pub_sub_group_connection";
        // var group = new PubSubGroupConnection();
        // await CreateIndex(groupTable, nameof(group.HubAndGroup), cancellationToken);
        // await CreateIndex(groupTable, nameof(group.HubAndGroupAndConnectionId), cancellationToken);
        // await CreateIndex(groupTable, nameof(group.HubAndConnectionId), cancellationToken);
        // await CreateIndex(groupTable, nameof(group.HubAndUserId), cancellationToken);
        // await CreateIndex(groupTable, nameof(group.HubAndGroupAndUserId), cancellationToken);
        // await WaitForActiveTable(groupTable, cancellationToken);
        //
        // var userTable = $"{_options.StackName}_pub_sub_group_user";
        // var user = new PubSubGroupUser();
        // await CreateIndex(userTable, nameof(user.HubAndUserId), cancellationToken);
        // await CreateIndex(userTable, nameof(user.HubAndGroupAndUserId), cancellationToken);
        // await WaitForActiveTable(userTable, cancellationToken);
    }

    private async Task CreateIndex(string tableName, string attributeName,
        CancellationToken cancellationToken, bool retry = true)
    {
        var indexName = $"{attributeName}_Index";

        var response = await _client.DescribeTableAsync(tableName, cancellationToken);

        if (response.Table.GlobalSecondaryIndexes.Exists(i => i.IndexName == indexName))
            return;
        
        Console.WriteLine($"Creating index on table {tableName} {indexName}");

        try
        {
            await _client.UpdateTableAsync(new UpdateTableRequest
            {
                TableName = response.Table.TableName,
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
        catch (LimitExceededException e)
        {
            if (retry)
            {
                Thread.Sleep(10000);
                await CreateIndex(tableName, attributeName, cancellationToken, false);
            }
            else
            {
                throw;
            }
        }
        
    }

    private async Task<DescribeTableResponse> WaitForActiveTable(string tableName, CancellationToken cancellationToken)
    {
        var table = await _client.DescribeTableAsync(tableName, cancellationToken);
        
        Console.WriteLine(tableName + " " + table.Table.TableStatus);
        
        while (table.Table.TableStatus != TableStatus.ACTIVE)
        {
            Thread.Sleep(10000);
            table = await _client.DescribeTableAsync(tableName, cancellationToken);
            Console.WriteLine(tableName + " " + table.Table.TableStatus);
        }

        return table;
    }
}