namespace Nuages.PubSub.API.Sdk;

public partial class PubSubServiceClient
{
    private readonly string _url;
    private readonly string _apiKey;
    private readonly string _hub;

    private HttpClient? _httpClient;

    private HttpClient HttpClient
    {
        get { return _httpClient ??= new HttpClient(); }
    }
    public PubSubServiceClient(string url, string apiKey, string hub)
    {
        _url = url;
        _apiKey = apiKey;
        _hub = hub;
    }
    
    public async Task<string> GetClientAccessTokenAsync(string userId, TimeSpan? expiresAfter = default, IEnumerable<string>? roles = null)
    {
        var webService = new AuthClient(HttpClient)
        {
            BaseUrl = _url
        };
    
        return await webService.GetClientAccessTokenAsync(userId, _hub, expiresAfter, roles);
    }
}