namespace Nuages.PubSub.API.Sdk;

public partial class PubSubServiceClient
{
    private readonly string _url;
    private readonly string _apiKey;
    private readonly string _hub;

    private HttpClient _httpClient;

    public PubSubServiceClient(string url, string apiKey, string hub)
    {
        _url = url;
        _apiKey = apiKey;
        _hub = hub;

        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="expiresAfter"></param>
    /// <param name="roles"></param>
    /// <returns></returns>
    public async Task<string> GetClientAccessTokenAsync(string userId, TimeSpan? expiresAfter = default, IEnumerable<string>? roles = null)
    {
        var webService = new AuthClient(_httpClient)
        {
            BaseUrl = _url
        };
    
        return await webService.GetClientAccessTokenAsync(userId, expiresAfter, roles).ConfigureAwait(false);
    }
    
    public async Task<string> GetClientAccessUriAsync(string userId, TimeSpan? expiresAfter = default, IEnumerable<string>? roles = null, string? token = null)
    {
        var webService = new AuthClient(_httpClient)
        {
            BaseUrl = _url
        };

        return await webService.GetClientAccessUriAsync(userId, _hub, expiresAfter, roles, token).ConfigureAwait(false);
    }
}