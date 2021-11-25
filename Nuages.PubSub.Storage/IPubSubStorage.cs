namespace Nuages.PubSub.Storage;

public interface IPubSubStorage
{
    Task InsertAsync(string hub, string connectionid, string sub,  TimeSpan? expireDelay = null);
    Task DeleteAsync(string hub, string connectionId);

    IEnumerable<string> GetAllConnectionIds(string hub);
    IEnumerable<string> GetAllConnectionForGroup(string hub, string group);

    Task Initialize();
}