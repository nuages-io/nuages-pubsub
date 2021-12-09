


namespace Nuages.PubSub.Storage.DynamoDb.DataModel
{
    public class PubSubAck 
    {
        public string Id { get; set; } = null!;
    
        public string ConnectionId { get; set; } = null!;
        public string Hub { get; set; } = null!;
        public string AckId { get; set; } = null!;

    }
}