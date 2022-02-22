using System.Diagnostics.CodeAnalysis;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.Extensions.Options;
using Nuages.PubSub.Services;
using Nuages.PubSub.Storage.DynamoDb.DataModel;

namespace Nuages.PubSub.Storage.DynamoDb;

public class DynamoDbStorage : PubSubStorgeBase<PubSubConnection>, IPubSubStorage
{
    private readonly DynamoDBContext _context;
    private readonly PubSubOptions _options;


    [ExcludeFromCodeCoverage]
    public void Initialize()
    {
    }

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

    public async IAsyncEnumerable<IPubSubConnection> GetAllConnectionAsync(string hub)
    {
        var search = _context.QueryAsync<PubSubConnection>(
            hub,
            GetOperationConfig()
        );

        while (!search.IsDone)
        {
            var list = await search.GetNextSetAsync();

            foreach (var item in list)
            {
                yield return item;
            }
        }
    }
    
    public async IAsyncEnumerable<string> GetConnectionsIdsForGroupAsync(string hub, string group)
    {
        var config = GetOperationConfig();

        var search = _context.QueryAsync<PubSubGroupConnection>(hub, QueryOperator.BeginsWith,
            new List<object> { group + "-" },
            config
        );

        while (!search.IsDone)
        {
            var list = await search.GetNextSetAsync();
            foreach (var item in list)
            {
                yield return item.ConnectionId;
            }
        }
    }

    public async Task<bool> GroupHasConnectionsAsync(string hub, string group)
    {
        var config = GetOperationConfig();

        var search = _context.QueryAsync<PubSubGroupConnection>(hub, QueryOperator.BeginsWith,
            new List<object> { group + "-" },
            config
        );

        return (await search.GetNextSetAsync()).Any();
    }

    public async Task<bool> UserHasConnectionsAsync(string hub, string userId)
    {
        var config = GetOperationConfig();
        config.IndexName = "Connection_UserId";

        var search = _context.QueryAsync<PubSubConnection>(hub, QueryOperator.Equal, new List<object> { userId },
            config
        );

        return (await search.GetNextSetAsync()).Any();
    }

    public async Task<bool> ConnectionExistsAsync(string hub, string connectionid)
    {
        var connection = await _context.LoadAsync<PubSubConnection>(hub, connectionid,
            GetOperationConfig());

        return connection != null;
    }

    public async Task RemoveConnectionFromGroupAsync(string hub, string group, string connectionId)
    {
        var config = GetOperationConfig();

        var groupConnection = await _context.LoadAsync<PubSubGroupConnection>(hub,
            group + "-" + connectionId,
            config
        );

        if (groupConnection != null)
            await _context.DeleteAsync(groupConnection, GetOperationConfig());
    }

    public async Task<bool> IsUserInGroupAsync(string hub, string group, string userId)
    {
        var config = GetOperationConfig();
        
        var search = await _context.LoadAsync<PubSubGroupUser>(hub,   group + "-" + userId,
            config);

        return search != null;
    }

    public async Task AddUserToGroupAsync(string hub, string group, string userId)
    {
        var search = await _context.LoadAsync<PubSubGroupUser>(hub, group + "-" + userId
            , GetOperationConfig());

        if (search == null)
        {
            var userConnection = new PubSubGroupUser
            {
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
        await _context.DeleteAsync<PubSubGroupUser>(hub, group + "-" + userId, GetOperationConfig());

        var config = GetOperationConfig();
        config.IndexName = "GroupConnection_GroupAndUserId";
        
        var search2 = _context.QueryAsync<PubSubGroupConnection>(hub, QueryOperator.Equal, new List<object>{ group + "-" + userId },config);

        while (!search2.IsDone)
        {
            var list2 = await search2.GetNextSetAsync();

            foreach (var c in list2)
            {
                await _context.DeleteAsync(c, GetOperationConfig());
            }
        }
       
    }

    public async Task RemoveUserFromAllGroupsAsync(string hub, string userId)
    {
        var config = GetOperationConfig();
        config.IndexName = "GroupUser_UserId";
        
        var search = _context.QueryAsync<PubSubGroupUser>(hub, QueryOperator.Equal, new List<object>
        {
            userId
        }, config);

        while (!search.IsDone)
        {
            var list = await search.GetNextSetAsync();

            foreach (var c in list)
            {
                await _context.DeleteAsync(c, GetOperationConfig());
            }
        }
        

        config.IndexName = "GroupConnection_UserId";
        
        var search2 = _context.QueryAsync<PubSubGroupConnection>(hub, QueryOperator.Equal, new List<object> { userId },
            config);

        while (!search2.IsDone)
        {
            var list2 = await search2.GetNextSetAsync();

            foreach (var c in list2)
            {
                await _context.DeleteAsync(c, GetOperationConfig());
            }
        }
       
    }

    public async IAsyncEnumerable<string> GetGroupsForUser(string hub, string userId)
    {
        var config = GetOperationConfig();
        config.IndexName = "GroupConnection_UserId";
        
        var search = _context.QueryAsync<PubSubGroupConnection>(hub, QueryOperator.Equal, new List<object> { userId },
           config);

        while (!search.IsDone)
        {
            var list = await search.GetNextSetAsync();
            foreach (var item in list)
            {
                yield return item.Group;
            }
        }
        
    }

    public async Task DeleteConnectionAsync(string hub, string connectionId)
    {
        await _context.DeleteAsync<PubSubConnection>(hub, connectionId, GetOperationConfig());

        var config = GetOperationConfig();
        config.IndexName = "GroupConnection_ConnectionId";
        
        //Delete from group
        var search2 =  _context.QueryAsync<PubSubGroupConnection>(hub, QueryOperator.Equal, new List<object>{connectionId},
           config);

        var list2 = await search2.GetNextSetAsync();
        foreach (var c in list2)
        {
            await _context.DeleteAsync(c, GetOperationConfig());
        }
        
        //Delete from ack
        var search3 =  _context.QueryAsync<PubSubAck>(hub, QueryOperator.BeginsWith, new List<object>{connectionId},
            GetOperationConfig());

        var list3 = await search3.GetNextSetAsync();
        foreach (var a in list3)
        {
            await _context.DeleteAsync(a, GetOperationConfig());
        }
    }

    public async Task<bool> ExistAckAsync(string hub, string connectionId, string ackId)
    {
        var ack = await _context.LoadAsync<PubSubAck>(hub, $"{connectionId}-{ackId}", GetOperationConfig());

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
        var config = GetOperationConfig();

        var groupConnection = await _context.LoadAsync<PubSubGroupConnection>(hub,
            group + "-" + connectionId,
            config
        );

        return groupConnection != null;
    }

    public override async Task<IPubSubConnection?> GetConnectionAsync(string hub, string connectionId)
    {
        return await _context.LoadAsync<PubSubConnection>(hub, connectionId, GetOperationConfig());
    }

    protected override async Task UpdateAsync(IPubSubConnection connection)
    {
        await _context.SaveAsync((PubSubConnection)connection, GetOperationConfig());
    }

    public override async IAsyncEnumerable<IPubSubConnection> GetConnectionsForUserAsync(string hub, string userId)
    {
        var config = GetOperationConfig();
        config.IndexName = "Connection_UserId";

        var search = _context.QueryAsync<PubSubConnection>(hub, QueryOperator.Equal, new List<object> { userId },
            config
        );

        while (!search.IsDone)
        {
            var list = await search.GetNextSetAsync();
            foreach (var item in list)
                yield return item;
        }
    }

    public override async Task AddConnectionToGroupAsync(string hub, string group, string connectionId)
    {
        var config = GetOperationConfig();

        var search = await _context.LoadAsync<PubSubGroupConnection>(hub, 
            group + "-" + connectionId,
            config
        );

        if (search == null)
        {
            var conn = await GetConnectionAsync(hub, connectionId);
            if (conn != null)
            {
                var groupConnection = new PubSubGroupConnection
                {
                    ConnectionId = connectionId,
                    Group = group,
                    Hub = hub,
                    CreatedOn = DateTime.UtcNow,
                    UserId = conn.UserId,
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

        await _context.SaveAsync(dynamoDbConn,
            GetOperationConfig());
    }

    [ExcludeFromCodeCoverage]
    public void TruncateAllData()
    {
        var groupConnectionToDelete = _context.ScanAsync<PubSubGroupConnection>(new List<ScanCondition>(), GetOperationConfig())
            .GetRemainingAsync().Result;
        groupConnectionToDelete.ForEach(c =>
        {
            _context.DeleteAsync(c,
                GetOperationConfig()).Wait();
        });

        var connectionToDelete = _context.ScanAsync<PubSubConnection>(new List<ScanCondition>(), GetOperationConfig())
            .GetRemainingAsync().Result;
        connectionToDelete.ForEach(c =>
        {
            _context.DeleteAsync(c,
                GetOperationConfig()).Wait();
        });

        var groupUserToDelete = _context.ScanAsync<PubSubGroupUser>(new List<ScanCondition>(), GetOperationConfig())
            .GetRemainingAsync().Result;
        groupUserToDelete.ForEach(c =>
        {
            _context.DeleteAsync(c,
                GetOperationConfig()).Wait();
        });

        var ackToDelete = _context.ScanAsync<PubSubAck>(new List<ScanCondition>(), GetOperationConfig()).GetRemainingAsync()
            .Result;
        ackToDelete.ForEach(c =>
        {
            _context.DeleteAsync(c,
                GetOperationConfig()).Wait();
        });
    }
}