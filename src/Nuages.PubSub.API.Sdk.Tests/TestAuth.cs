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
        var client = new PubSubServiceClient(TestUrl, TestApiKey, TestHub);

       var token = await client.GetClientAccessTokenAsync(TestUserId, null, new List<string> { nameof(PubSubPermission.SendMessageToGroup), nameof(PubSubPermission.JoinOrLeaveGroup) });
       
       TestOutputHelper.WriteLine($"token={token}");
    }
    
    [Fact]
    public async Task ShouldGetAuthUrl()
    {
        var client = new PubSubServiceClient(TestUrl, TestApiKey, TestHub);
    
        var url = await client.GetClientAccessUriAsync(TestUserId, null, new List<string> { nameof(PubSubPermission.SendMessageToGroup), nameof(PubSubPermission.JoinOrLeaveGroup)});
       
        TestOutputHelper.WriteLine($"url={url}");
    }

    public TestAuth(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }
}