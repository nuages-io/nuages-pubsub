namespace Nuages.PubSub.API.Sdk;

public class PubSubServiceClient
{
    private readonly string _url;
    private readonly string _apiKey;
    private readonly string _hub;
    
    public PubSubServiceClient(string apiUrl, string apiKey, string hub)
    {
        _url = apiUrl;
        _apiKey = apiKey;
        _hub = hub;
    }
}