using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Nuages.PubSub.Storage.Mongo;

public partial interface IMongoClientProvider
{
    MongoClient GetMongoClient(string connectionString);
}

public class MongoClientProvider : IMongoClientProvider
{
    public readonly Dictionary<string, MongoClient> _clients = new Dictionary<string, MongoClient>();

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