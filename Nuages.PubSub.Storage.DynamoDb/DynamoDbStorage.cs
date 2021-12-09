using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
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

    public Task<IEnumerable<IPubSubConnection>> GetAllConnectionAsync(string hub)
    {
        throw new NotImplementedException();
    }


    public Task<IEnumerable<IPubSubConnection>> GetConnectionsForGroupAsync(string hub, string @group)
    {
        throw new NotImplementedException();
    }

    public Task<bool> GroupHasConnectionsAsync(string hub, string @group)
    {
        throw new NotImplementedException();
    }


    public Task<bool> UserHasConnectionsAsync(string hub, string userId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ConnectionExistsAsync(string hub, string connectionid)
    {
        throw new NotImplementedException();
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

    public override Task<IEnumerable<IPubSubConnection>> GetConnectionsForUserAsync(string hub, string userId)
    {
        throw new NotImplementedException();
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
}