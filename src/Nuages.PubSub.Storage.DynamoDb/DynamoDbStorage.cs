using System.Diagnostics.CodeAnalysis;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.Extensions.Options;
using Nuages.PubSub.Services;
using Nuages.PubSub.Services.Storage;
using Nuages.PubSub.Storage.DynamoDb.DataModel;

namespace Nuages.PubSub.Storage.DynamoDb;

public class DynamoDbStorage : PubSubStorgeBase<PubSubConnection>, IPubSubStorage
{
    private readonly DynamoDBContext _context;
    private readonly PubSubOptions _options;

    public DynamoDbStorage(IOptions<PubSubOptions> options)
    {
        _options = options.Value;
        
        var client = new AmazonDynamoDBClient();
        _context = new DynamoDBContext(client);
    }

    private DynamoDBOperationConfig GetOperationConfig()
    {
        return new DynamoDBOperationConfig
        {
            TableNamePrefix = $"{_options.StackName}_"
        };
    }
    
    /// <summary>
    /// Get all connection for hub
    /// </summary>
    /// <param name="hub"></param>
    /// <returns></returns>
    public async Task<IEnumerable<IPubSubConnection>> GetAllConnectionAsync(string hub)
    {
        var search = _context.QueryAsync<PubSubConnection>(
            hub,
            GetOperationConfig()
        );

        return await search.GetRemainingAsync();
    }


    public async Task<IEnumerable<string>> GetConnectionsIdsForGroupAsync(string hub, string group)
    {
        var search = _context.ScanAsync<PubSubGroupConnection>(
            new List<ScanCondition>
            {
                new ("Hub",  ScanOperator.Equal, hub),
                new ("Group",  ScanOperator.Equal, group)
                
            },
            GetOperationConfig()
        );

        var groupConnections = await search.GetRemainingAsync();

        return  groupConnections.Where(c => !c.IsExpired()).Select(c => c.ConnectionId);
    }

    public async Task<bool> GroupHasConnectionsAsync(string hub, string group)
    {
        var search = _context.ScanAsync<PubSubGroupConnection>(
            new List<ScanCondition>
            {
                new ("Hub",  ScanOperator.Equal, hub),
                new ("Group",  ScanOperator.Equal, group)
            },
            GetOperationConfig()
        );
        
        var list = await search.GetNextSetAsync();
        
        return list.Any();
    }


    public async Task<bool> UserHasConnectionsAsync(string hub, string userId)
    {
        var search = _context.ScanAsync<PubSubConnection>(
            new List<ScanCondition>
            {
                new ("Hub",  ScanOperator.Equal, hub),
                new ("UserId",  ScanOperator.Equal, userId)
            },
            GetOperationConfig()
        );
        
        var list = await search.GetNextSetAsync();

        return list?.Any() ?? false;

    }

    public async Task<bool> ConnectionExistsAsync(string hub, string connectionid)
    {
        var connection =  await _context.LoadAsync<PubSubConnection>(hub, connectionid,
            GetOperationConfig());

        return connection != null;
    }

    public async Task RemoveConnectionFromGroupAsync(string hub, string group, string connectionId)
    {
        var search =  _context.ScanAsync<PubSubGroupConnection>(new List<ScanCondition>
        {
            new ("Hub",  ScanOperator.Equal, hub),
            new ("Group",  ScanOperator.Equal, group),
            new ("ConnectionId",  ScanOperator.Equal, connectionId)
        },
            GetOperationConfig());

        var list = await search.GetRemainingAsync();

        foreach (var c in list)
        {
            await _context.DeleteAsync(c,GetOperationConfig());
        }
       
    }

    public async Task<bool> IsUserInGroupAsync(string hub, string group, string userId)
    {
        var search =  _context.ScanAsync<PubSubGroupUser>(new List<ScanCondition>
            {
                new ("Hub",  ScanOperator.Equal, hub),
                new ("Group",  ScanOperator.Equal, group),
                new ("UserId",  ScanOperator.Equal, userId)
            },
            GetOperationConfig());

       return (await search.GetRemainingAsync()).Any();

    }

    public async Task AddUserToGroupAsync(string hub, string group, string userId)
    {
        var search =  _context.ScanAsync<PubSubGroupUser>(new List<ScanCondition>
        {
            new ("Hub",  ScanOperator.Equal, hub),
            new ("Group",  ScanOperator.Equal, group),
            new ("UserId",  ScanOperator.Equal, userId)
        },
            GetOperationConfig());
        
        var list = await search.GetRemainingAsync();
        
        if (!list.Any())
        {
            var userConnection = new PubSubGroupUser
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Group = group,
                Hub = hub,
                CreatedOn = DateTime.Now
            };

            userConnection.Initialize();
            
            await _context.SaveAsync(userConnection, GetOperationConfig());
        }

        await AddConnectionToGroupFromUserGroups(hub, group, userId);
    }

    public async Task RemoveUserFromGroupAsync(string hub, string group, string userId)
    {
        var search =  _context.ScanAsync<PubSubGroupUser>(new List<ScanCondition>
        {
            new ("Hub",  ScanOperator.Equal, hub),
            new ("Group",  ScanOperator.Equal, group),
            new ("UserId",  ScanOperator.Equal, userId)
        },
            GetOperationConfig());
        
        var list = await search.GetRemainingAsync();

        foreach (var c in list)
        {
            await _context.DeleteAsync(c, GetOperationConfig());
        }
        
        var search2 =  _context.ScanAsync<PubSubGroupConnection>(new List<ScanCondition>
        {
            new ("Hub",  ScanOperator.Equal, hub),
            new ("Group",  ScanOperator.Equal, group),
            new ("UserId",  ScanOperator.Equal, userId)
        },
            GetOperationConfig());
        
        var list2 = await search2.GetRemainingAsync();

        foreach (var c in list2)
        {
            await _context.DeleteAsync(c, GetOperationConfig());
        }
    }

    public async Task RemoveUserFromAllGroupsAsync(string hub, string userId)
    {
        var search =  _context.ScanAsync<PubSubGroupUser>(new List<ScanCondition>
        {
            new ("Hub",  ScanOperator.Equal, hub),
            new ("UserId",  ScanOperator.Equal, userId)
        },
            GetOperationConfig());
        
        var list = await search.GetRemainingAsync();

        foreach (var c in list)
        {
            await _context.DeleteAsync(c, GetOperationConfig());
        }
        
        var search2 =  _context.ScanAsync<PubSubGroupConnection>(new List<ScanCondition>
        {
            new ("Hub",  ScanOperator.Equal, hub),
            new ("UserId",  ScanOperator.Equal, userId)
        },
            GetOperationConfig());
        
        var list2 = await search2.GetRemainingAsync();

        foreach (var c in list2)
        {
            await _context.DeleteAsync(c, GetOperationConfig());
        }
        
    }

    public async Task<IEnumerable<string>> GetGroupsForUser(string hub, string userId)
    {
        var search =  _context.ScanAsync<PubSubGroupConnection>(new List<ScanCondition>
        {
            new ("Hub",  ScanOperator.Equal, hub),
            new ("UserId",  ScanOperator.Equal, userId)
        },
            GetOperationConfig());
        
        var list = await search.GetRemainingAsync();

        return list.Select(c => c.Group);
    }

    public async Task DeleteConnectionAsync(string hub, string connectionId)
    {
        await _context.DeleteAsync<PubSubConnection>(hub, connectionId, GetOperationConfig());
        
        
        var search2 =  _context.ScanAsync<PubSubGroupConnection>(new List<ScanCondition>
        {
            new ("Hub",  ScanOperator.Equal, hub),
            new ("ConnectionId",  ScanOperator.Equal, connectionId)
        },
            GetOperationConfig());

        var list2 = await search2.GetNextSetAsync();
        foreach (var c in list2)
        {
            await _context.DeleteAsync(c, GetOperationConfig());
        }
    }

    public async Task<bool> ExistAckAsync(string hub, string connectionId, string ackId)
    {
        var ack = await _context.LoadAsync<PubSubAck>(hub,  $"{connectionId}-{ackId}", GetOperationConfig());
        
        return ack != null;
    }

    public async Task InsertAckAsync(string hub, string connectionId, string ackId)
    {
        var pubSubAck = new PubSubAck
        {
            AckId = ackId,
            ConnectionId = connectionId,
            Hub = hub
        };
        
        pubSubAck.Initialize();

        await _context.SaveAsync(pubSubAck,
            GetOperationConfig());
    }

    public async Task<bool> IsConnectionInGroup(string hub, string group, string connectionId)
    {
        var search =  _context.ScanAsync<PubSubGroupConnection>(new List<ScanCondition>
        {
            new ("Hub",  ScanOperator.Equal, hub),
            new ("Group",  ScanOperator.Equal, group),
            new ("ConnectionId",  ScanOperator.Equal, connectionId)
        },
            GetOperationConfig());

        var list = await search.GetNextSetAsync();

        return list.Any();
    }

    public override async Task<IPubSubConnection?> GetConnectionAsync(string hub, string connectionId)
    {
        return await _context.LoadAsync<PubSubConnection>(hub, connectionId, GetOperationConfig());
    }

    protected override async Task UpdateAsync(IPubSubConnection connection)
    {
        await _context.SaveAsync((PubSubConnection)connection, GetOperationConfig());
    }

    public override async Task<IEnumerable<IPubSubConnection>> GetConnectionsForUserAsync(string hub, string userId)
    {
        var config = GetOperationConfig();
        config.IndexName = "Connection_UserId";
        
        var search = _context.QueryAsync<PubSubConnection>(hub, QueryOperator.Equal, new List<object>{ userId },
            config
        );

        var connections = new List<PubSubConnection>();
        
        while (!search.IsDone)
        {
            var list = await search.GetNextSetAsync();
            connections.AddRange(list);
        }
        
        return connections;
    }

    public override async Task AddConnectionToGroupAsync(string hub, string group, string connectionId, string userId)
    {
        var search =  _context.ScanAsync<PubSubGroupConnection>(new List<ScanCondition>
        {
            new ("Hub",  ScanOperator.Equal, hub),
            new ("Group",  ScanOperator.Equal, group),
            new ("ConnectionId",  ScanOperator.Equal, connectionId)
        },
            GetOperationConfig());

        var list = await search.GetNextSetAsync();

        if (!list.Any())
        {

            var conn = await GetConnectionAsync(hub, connectionId);
            if (conn != null)
            {
                var groupConnection = new PubSubGroupConnection
                {
                    Id = Guid.NewGuid().ToString(),
                    ConnectionId = connectionId,
                    Group = group,
                    Hub = hub,
                    CreatedOn = DateTime.UtcNow,
                    UserId = userId,
                    ExpireOn = conn.ExpireOn
                };
                
                groupConnection.Initialize();

                await _context.SaveAsync(groupConnection, GetOperationConfig());
            }
          
        }
    }

    protected override async Task InsertAsync(IPubSubConnection conn)
    {
        var dynamoDbConn = (PubSubConnection)conn;
        
        //dynamoDbConn.Initialize();
        
        await _context.SaveAsync(dynamoDbConn,
            GetOperationConfig());
    }

    protected override string GetNewId()
    {
        return Guid.NewGuid().ToString();
    }

    [ExcludeFromCodeCoverage]
    public void DeleteAll()
    {
        var list4 = _context.ScanAsync<PubSubGroupConnection>(new List<ScanCondition>(), GetOperationConfig()).GetRemainingAsync().Result;
        list4.ForEach(c =>
        {
            _context.DeleteAsync(c,
                GetOperationConfig()).Wait();
        });
        
        var list =  _context.ScanAsync<PubSubConnection>(new List<ScanCondition>(), GetOperationConfig()).GetRemainingAsync().Result;
        list.ForEach(c =>
        {
            _context.DeleteAsync(c,
                GetOperationConfig()).Wait();
        });
        
        var list2 = _context.ScanAsync<PubSubGroupUser>(new List<ScanCondition>(), GetOperationConfig()).GetRemainingAsync().Result;
        list2.ForEach(c =>
        {
            _context.DeleteAsync(c,
                GetOperationConfig()).Wait();
        });
        
        var list3 = _context.ScanAsync<PubSubAck>(new List<ScanCondition>(), GetOperationConfig()).GetRemainingAsync().Result;
        list3.ForEach(c =>
        {
            _context.DeleteAsync(c,
                GetOperationConfig()).Wait();
        });
        
       
    }
}