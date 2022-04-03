using MongoDB.Driver;

namespace Nuages.PubSub.Storage.Mongo;

public interface IMongoClientProvider
{
    MongoClient GetMongoClient(string connectionString);
}

public class MongoClientProvider : IMongoClientProvider
{
    private readonly Dictionary<string, MongoClient> _clients = new ();

    public MongoClient GetMongoClient(string connectionString)
    {
        if (!_clients.ContainsKey(connectionString))
        {
            var client = new MongoClient(connectionString);
            _clients[connectionString] = client;
        }

        return _clients[connectionString];
    }
}