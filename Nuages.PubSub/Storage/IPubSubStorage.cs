namespace Nuages.PubSub.Storage;

public interface IPubSubStorage
{
    Task InsertAsync(string connectionid, string sub);
    Task DeleteAsync(string connectionId);

    IEnumerable<string> GetAllConnectionIds();
}