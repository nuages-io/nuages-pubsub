namespace Nuages.PubSub.API.Sdk;

public partial class PubSubServiceClient
{
    private readonly string _url;
    private readonly string _apiKey;
    private readonly string _audience;

    private HttpClient? _httpClient = null;

    HttpClient HttpClient
    {
        get { return _httpClient ??= new HttpClient(); }
    }
    public PubSubServiceClient(string url, string apiKey, string audience)
    {
        _url = url;
        _apiKey = apiKey;
        _audience = audience;
    }
    
    public async Task<string> GetClientAccessTokenAsync(string userId, TimeSpan? expiresAfter = null, IEnumerable<string>? roles = null)
    {
        var webService = new AuthClient(HttpClient)
        {
            BaseUrl = _url
        };
    
        return await webService.GetClientAccessTokenAsync(userId, _audience, expiresAfter, roles);
    }
}