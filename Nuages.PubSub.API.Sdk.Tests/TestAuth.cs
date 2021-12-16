using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Nuages.PubSub.API.Sdk.Tests;

public class TestAuth : BaseTest
{
    
    
    [Fact]
    public async Task ShouldGetAuthToken()
    {
        var client = new PubSubServiceClient(_url, _apiKey, _hub);

       var token = await client.GetClientAccessTokenAsync(_userId, null, new List<string> { nameof(PubSubPermission.SendMessageToGroup), nameof(PubSubPermission.JoinOrLeaveGroup) });
       
       _testOutputHelper.WriteLine($"token={token}");
    }
    
    [Fact]
    public async Task ShouldGetAuthUrl()
    {
        var client = new PubSubServiceClient(_url, _apiKey, _hub);
    
        var url = await client.GetClientAccessUriAsync(_userId, null, new List<string> { nameof(PubSubPermission.SendMessageToGroup), nameof(PubSubPermission.JoinOrLeaveGroup)});
       
        _testOutputHelper.WriteLine($"url={url}");
    }

    public TestAuth(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }
}