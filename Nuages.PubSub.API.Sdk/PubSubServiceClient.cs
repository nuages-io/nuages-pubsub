namespace Nuages.PubSub.API.Sdk;

public class PubSubServiceClient
{
    private readonly string _url;
    private readonly string _apiKey;
    private readonly string _audience;
    
    public PubSubServiceClient(string url, string apiKey, string audience)
    {
        _url = url;
        _apiKey = apiKey;
        _audience = audience;
    }
    
    public async Task<string> GetClientAccessToken(string userId, TimeSpan? expiresAfter = null, IEnumerable<string>? roles = null)
    {
        var httpClient = new HttpClient();

        var webService = new AuthClient(httpClient)
        {
            BaseUrl = _url
        };

        return await webService.GetClientAccessTokenAsync(userId, _audience, expiresAfter, roles);
        

    }
}