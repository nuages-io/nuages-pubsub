using Nuages.PubSub.Services.Storage;

namespace Nuages.PubSub.Storage.DynamoDb;

public class DynamoDbStorage : IPubSubStorage
{
    public Task<IPubSubConnection> CreateConnectionAsync(string hub, string connectionid, string sub, TimeSpan? expireDelay)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<IPubSubConnection>> GetAllConnectionAsync(string hub)
    {
        throw new NotImplementedException();
    }

    public Task<IPubSubConnection?> GetConnectionAsync(string hub, string connectionId)
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

    public Task<IEnumerable<IPubSubConnection>> GetConnectionsForUserAsync(string hub, string userId)
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

    public Task AddPermissionAsync(string hub, string connectionId, string permissionString)
    {
        throw new NotImplementedException();
    }

    public Task RemovePermissionAsync(string hub, string connectionId, string permissionString)
    {
        throw new NotImplementedException();
    }

    public Task<bool> HasPermissionAsync(string hub, string connectionId, string permissionString)
    {
        throw new NotImplementedException();
    }

    public Task AddConnectionToGroupAsync(string hub, string @group, string connectionId, string userId)
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
}