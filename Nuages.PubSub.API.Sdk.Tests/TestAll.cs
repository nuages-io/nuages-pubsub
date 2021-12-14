using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Nuages.PubSub.API.Sdk.Tests;

public class TestAll : BaseTest
{
    public TestAll(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }
    
    [Fact]
    public async Task ShouldSendToAllAsync()
    {
        var client = new PubSubServiceClient(_url, _apiKey, _hub);

        await client.SendToAllAsync(new Message
        {
            Data = new
            {
                Message = "Yo men!",

            }
        });
    }

    [Fact]
    public async Task ShouldCloseAllAsync()
    {
        var client = new PubSubServiceClient(_url, _apiKey, _hub);

        await client.CloseAllConnectionsAsync();
    }
}