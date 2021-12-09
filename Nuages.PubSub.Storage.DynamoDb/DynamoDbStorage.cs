using System.Net;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Nuages.PubSub.Services.Storage;
using Nuages.PubSub.Storage.DynamoDb.DataModel;

namespace Nuages.PubSub.Storage.DynamoDb;

public class DynamoDbStorage : PubSubStorgeBase<PubSubConnection>, IPubSubStorage
{
    private readonly AmazonDynamoDBClient _client;
    private readonly DynamoDBContext _context;

    public DynamoDbStorage()
    {
        _client = new AmazonDynamoDBClient();
        _context = new DynamoDBContext(_client);

    }

    public async Task<IEnumerable<IPubSubConnection>> GetAllConnectionAsync(string hub)
    {
        var res = _context.ScanAsync<PubSubConnection>(
            new List<ScanCondition>
            {
                new ("Hub",  ScanOperator.Equal, new object[] { hub })
            }
        );

        List<PubSubConnection> connection = new List<PubSubConnection>();
        
        while (!res.IsDone)
        {
            var list = await res.GetNextSetAsync();
            connection.AddRange(list);
        }

        return connection;
    }


    public async Task<IEnumerable<IPubSubConnection>> GetConnectionsForGroupAsync(string hub, string group)
    {
        var res = _context.ScanAsync<PubSubGroupConnection>(
            new List<ScanCondition>
            {
                new ("Group",  ScanOperator.Equal, new object[] { group }),
                new ("Hub",  ScanOperator.Equal, new object[] { hub })
            }
        );

        List<PubSubGroupConnection> groupConnections = new List<PubSubGroupConnection>();
        
        while (!res.IsDone)
        {
            var list = await res.GetNextSetAsync();
            groupConnections.AddRange(list);
        }

        var res2 = _context.ScanAsync<PubSubConnection>(
            new List<ScanCondition>
            {
                new ("ConnectionId",  ScanOperator.Equal, groupConnections.Select(g => g.ConnectionId)),
                new ("Hub",  ScanOperator.Equal, new object[] { hub })
            }
        );

        List<PubSubConnection> connections = new List<PubSubConnection>();
        
        while (!res2.IsDone)
        {
            var list = await res2.GetNextSetAsync();
            connections.AddRange(list);
        }
        
        return connections;
    }

    public Task<bool> GroupHasConnectionsAsync(string hub, string @group)
    {
        throw new NotImplementedException();
    }


    public async Task<bool> UserHasConnectionsAsync(string hub, string userId)
    {
        var res = _context.ScanAsync<PubSubConnection>(
            new List<ScanCondition>
            {
                new ("Sub",  ScanOperator.Equal, userId),
                new ("Hub",  ScanOperator.Equal, hub)
            }
        );
        
        var list = await res.GetNextSetAsync();

        return list?.Any() ?? false;

    }

    public async Task<bool> ConnectionExistsAsync(string hub, string connectionid)
    {
        var res =  _context.ScanAsync<PubSubConnection>(new List<ScanCondition>
        {
            new ("ConnectionId",  ScanOperator.Equal, new object[] { connectionid }),
            new ("Hub",  ScanOperator.Equal, new object[] { hub })
        });

        var nextSet = await res.GetNextSetAsync();

        return nextSet?.Any() ?? false;
    }

    public Task RemoveConnectionFromGroupAsync(string hub, string @group, string connectionId)
    {
        throw new NotImplementedException();
    }

    public Task AddUserToGroupAsync(string hub, string @group, string userId)
    {
        throw new NotImplementedException();
    }

    public Task RemoveUserFromGroupAsync(string hub, string @group, string userId)
    {
        throw new NotImplementedException();
    }

    public Task RemoveUserFromAllGroupsAsync(string hub, string userId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<string>> GetGroupsForUser(string hub, string sub)
    {
        throw new NotImplementedException();
    }

    public Task DeleteConnectionAsync(string hub, string connectionId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistAckAsync(string hub, string connectionId, string ackId)
    {
        throw new NotImplementedException();
    }

    public Task InsertAckAsync(string hub, string connectionId, string ackId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsConnectionInGroup(string hub, string @group, string connectionId)
    {
        throw new NotImplementedException();
    }

    public override Task<IPubSubConnection?> GetConnectionAsync(string hub, string connectionId)
    {
        throw new NotImplementedException();
    }

    protected override Task UpdateAsync(IPubSubConnection connection)
    {
        throw new NotImplementedException();
    }

    public override async Task<IEnumerable<IPubSubConnection>> GetConnectionsForUserAsync(string hub, string userId)
    {
        var res = _context.ScanAsync<PubSubConnection>(
            new List<ScanCondition>
            {
                new ("Sub",  ScanOperator.Equal, userId),
                new ("Hub",  ScanOperator.Equal, hub)
            }
        );

        List<PubSubConnection> connections = new List<PubSubConnection>();
        
        while (!res.IsDone)
        {
            var list = await res.GetNextSetAsync();
            connections.AddRange(list);
        }
        
        return connections;
    }

    public override Task AddConnectionToGroupAsync(string hub, string @group, string connectionId, string userId)
    {
        throw new NotImplementedException();
    }

    protected override async Task InsertAsync(IPubSubConnection conn)
    {
        await _context.SaveAsync((PubSubConnection) conn);
    }

    protected override string GetNewId()
    {
        return Guid.NewGuid().ToString();
    }

    public void DeleteAll()
    {
        var list =  _context.ScanAsync<PubSubConnection>(new List<ScanCondition>()).GetRemainingAsync().Result;
        list.ForEach(c =>
        {
            _context.DeleteAsync(c).Wait();
        });
        
        var list2 = _context.ScanAsync<PubSubGroupUser>(new List<ScanCondition>()).GetRemainingAsync().Result;
        list2.ForEach(c =>
        {
            _context.DeleteAsync(c).Wait();
        });
        
        var list3 = _context.ScanAsync<PubSubAck>(new List<ScanCondition>()).GetRemainingAsync().Result;
        list3.ForEach(c =>
        {
            _context.DeleteAsync(c).Wait();
        });
        
        var list4 = _context.ScanAsync<PubSubGroupConnection>(new List<ScanCondition>()).GetRemainingAsync().Result;
        list4.ForEach(c =>
        {
            _context.DeleteAsync(c).Wait();
        });
    }
}