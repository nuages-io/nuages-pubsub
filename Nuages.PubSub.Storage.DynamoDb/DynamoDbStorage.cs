using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Nuages.PubSub.Services.Storage;
using Nuages.PubSub.Storage.DynamoDb.DataModel;

namespace Nuages.PubSub.Storage.DynamoDb;

public class DynamoDbStorage : PubSubStorgeBase<PubSubConnection>, IPubSubStorage
{
    private readonly DynamoDBContext _context;

    public DynamoDbStorage()
    {
        var client = new AmazonDynamoDBClient();
        _context = new DynamoDBContext(client);
        
    }

    public async Task<IEnumerable<IPubSubConnection>> GetAllConnectionAsync(string hub)
    {
        var search = _context.ScanAsync<PubSubConnection>(
            new List<ScanCondition>
            {
                new ("Hub",  ScanOperator.Equal, hub)
            }
        );

        return await search.GetRemainingAsync();
    }


    public async Task<IEnumerable<string>> GetConnectionsIdsForGroupAsync(string hub, string group)
    {
        var search = _context.ScanAsync<PubSubGroupConnection>(
            new List<ScanCondition>
            {
                new ("Group",  ScanOperator.Equal, group),
                new ("Hub",  ScanOperator.Equal, hub)
            }
        );

        var groupConnections = await search.GetRemainingAsync();

        return  groupConnections.Where(c => !c.IsExpired()).Select(c => c.ConnectionId);
    }

    public async Task<bool> GroupHasConnectionsAsync(string hub, string group)
    {
        var search = _context.ScanAsync<PubSubGroupConnection>(
            new List<ScanCondition>
            {
                new ("Group",  ScanOperator.Equal, group),
                new ("Hub",  ScanOperator.Equal, hub)
            }
        );

        
        var list = await search.GetNextSetAsync();
        
        return list.Any();
    }


    public async Task<bool> UserHasConnectionsAsync(string hub, string userId)
    {
        var search = _context.ScanAsync<PubSubConnection>(
            new List<ScanCondition>
            {
                new ("Sub",  ScanOperator.Equal, userId),
                new ("Hub",  ScanOperator.Equal, hub)
            }
        );
        
        var list = await search.GetNextSetAsync();

        return list?.Any() ?? false;

    }

    public async Task<bool> ConnectionExistsAsync(string hub, string connectionid)
    {
        var search =  _context.ScanAsync<PubSubConnection>(new List<ScanCondition>
        {
            new ("ConnectionId",  ScanOperator.Equal, connectionid),
            new ("Hub",  ScanOperator.Equal, hub)
        });

        var list = await search.GetNextSetAsync();

        return list?.Any() ?? false;
    }

    public async Task RemoveConnectionFromGroupAsync(string hub, string group, string connectionId)
    {
        var search =  _context.ScanAsync<PubSubGroupConnection>(new List<ScanCondition>
        {
            new ("ConnectionId",  ScanOperator.Equal, connectionId),
            new ("Group",  ScanOperator.Equal, group),
            new ("Hub",  ScanOperator.Equal, hub)
        });

        var list = await search.GetRemainingAsync();

        foreach (var c in list)
        {
            await _context.DeleteAsync(c);
        }
       
    }

    public async Task AddUserToGroupAsync(string hub, string group, string userId)
    {
        var search =  _context.ScanAsync<PubSubGroupUser>(new List<ScanCondition>
        {
            new ("Sub",  ScanOperator.Equal, userId),
            new ("Group",  ScanOperator.Equal, group),
            new ("Hub",  ScanOperator.Equal, hub)
        });
        
        var list = await search.GetRemainingAsync();
        
        if (!list.Any())
        {
            var userConnection = new PubSubGroupUser
            {
                Id = Guid.NewGuid().ToString(),
                Sub = userId,
                Group = group,
                Hub = hub,
                CreatedOn = DateTime.Now
            };

            await _context.SaveAsync(userConnection);
        }

        await AddConnectionToGroupFromUserGroups(hub, group, userId);
    }

    public async Task RemoveUserFromGroupAsync(string hub, string group, string userId)
    {
        var search =  _context.ScanAsync<PubSubGroupUser>(new List<ScanCondition>
        {
            new ("Sub",  ScanOperator.Equal, userId),
            new ("Group",  ScanOperator.Equal, group),
            new ("Hub",  ScanOperator.Equal, hub)
        });
        
        var list = await search.GetRemainingAsync();

        foreach (var c in list)
        {
            await _context.DeleteAsync(c);
        }
        
        var search2 =  _context.ScanAsync<PubSubGroupConnection>(new List<ScanCondition>
        {
            new ("Sub",  ScanOperator.Equal, userId),
            new ("Group",  ScanOperator.Equal, group),
            new ("Hub",  ScanOperator.Equal, hub)
        });
        
        var list2 = await search2.GetRemainingAsync();

        foreach (var c in list2)
        {
            await _context.DeleteAsync(c);
        }
    }

    public async Task RemoveUserFromAllGroupsAsync(string hub, string userId)
    {
        var search =  _context.ScanAsync<PubSubGroupUser>(new List<ScanCondition>
        {
            new ("Sub",  ScanOperator.Equal, userId),
            new ("Hub",  ScanOperator.Equal, hub)
        });
        
        var list = await search.GetRemainingAsync();

        foreach (var c in list)
        {
            await _context.DeleteAsync(c);
        }
        
        var search2 =  _context.ScanAsync<PubSubGroupConnection>(new List<ScanCondition>
        {
            new ("Sub",  ScanOperator.Equal, userId),
            new ("Hub",  ScanOperator.Equal, hub)
        });
        
        var list2 = await search2.GetRemainingAsync();

        foreach (var c in list2)
        {
            await _context.DeleteAsync(c);
        }
        
    }

    public async Task<IEnumerable<string>> GetGroupsForUser(string hub, string sub)
    {
        var search =  _context.ScanAsync<PubSubGroupConnection>(new List<ScanCondition>
        {
            new ("Sub",  ScanOperator.Equal, sub),
            new ("Hub",  ScanOperator.Equal, hub)
        });
        
        var list = await search.GetRemainingAsync();

        return list.Select(c => c.Group);
    }

    public async Task DeleteConnectionAsync(string hub, string connectionId)
    {
        var search =  _context.ScanAsync<PubSubConnection>(new List<ScanCondition>
        {
            new ("ConnectionId",  ScanOperator.Equal, connectionId),
            new ("Hub",  ScanOperator.Equal, hub)
        });

        var list = await search.GetNextSetAsync();
        foreach (var c in list)
        {
            await _context.DeleteAsync(c);
        }
        
        var search2 =  _context.ScanAsync<PubSubGroupConnection>(new List<ScanCondition>
        {
            new ("ConnectionId",  ScanOperator.Equal, connectionId),
            new ("Hub",  ScanOperator.Equal, hub)
        });

        var list2 = await search2.GetNextSetAsync();
        foreach (var c in list2)
        {
            await _context.DeleteAsync(c);
        }
    }

    public async Task<bool> ExistAckAsync(string hub, string connectionId, string ackId)
    {
        var search =  _context.ScanAsync<PubSubAck>(new List<ScanCondition>
        {
            new ("ConnectionId",  ScanOperator.Equal, connectionId),
            new ("AckId",  ScanOperator.Equal, ackId),
            new ("Hub",  ScanOperator.Equal, hub)
        });

        var list = await search.GetNextSetAsync();

        return list.Any();
    }

    public async Task InsertAckAsync(string hub, string connectionId, string ackId)
    {
        var pubSubAck = new PubSubAck
        {
            Id = Guid.NewGuid().ToString(),
            AckId = ackId,
            ConnectionId = connectionId,
            Hub = hub
        };

        await _context.SaveAsync(pubSubAck);
    }

    public async Task<bool> IsConnectionInGroup(string hub, string group, string connectionId)
    {
        var search =  _context.ScanAsync<PubSubGroupConnection>(new List<ScanCondition>
        {
            new ("ConnectionId",  ScanOperator.Equal, connectionId),
            new ("Group",  ScanOperator.Equal, group),
            new ("Hub",  ScanOperator.Equal, hub)
        });

        var list = await search.GetNextSetAsync();

        return list.Any();
    }

    public override async Task<IPubSubConnection?> GetConnectionAsync(string hub, string connectionId)
    {
        var search =  _context.ScanAsync<PubSubConnection>(new List<ScanCondition>
        {
            new ("ConnectionId",  ScanOperator.Equal, connectionId),
            new ("Hub",  ScanOperator.Equal, hub)
        });

        var list = await search.GetNextSetAsync();

        return list.Any() ? list.Single() : null;
    }

    protected override async Task UpdateAsync(IPubSubConnection connection)
    {
        await _context.SaveAsync((PubSubConnection)connection);
    }

    public override async Task<IEnumerable<IPubSubConnection>> GetConnectionsForUserAsync(string hub, string userId)
    {
        var search = _context.ScanAsync<PubSubConnection>(
            new List<ScanCondition>
            {
                new ("Sub",  ScanOperator.Equal, userId),
                new ("Hub",  ScanOperator.Equal, hub)
            }
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
            new ("ConnectionId",  ScanOperator.Equal, connectionId),
            new ("Group",  ScanOperator.Equal, group),
            new ("Hub",  ScanOperator.Equal, hub)
        });

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
                    Sub = userId,
                    ExpireOn = conn.ExpireOn
                };

                await _context.SaveAsync(groupConnection);
            }
          
        }
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