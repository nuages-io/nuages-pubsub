using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Amazon.ApiGatewayManagementApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Nuages.PubSub.Services;
using Nuages.PubSub.Services.Tests;
using Nuages.PubSub.Storage.EntityFramework;
using Xunit;

namespace NUages.PubSub.Storage.EntityFramework.Tests;

public class TestsPubSubServiceEntityFramework
{
    private readonly IPubSubService _pubSubService;
    private readonly string _hub;
    private readonly string _group;
    private readonly string _connectionId;
    private readonly string _userId;
    private readonly ServiceProvider _serviceProvider;

    public TestsPubSubServiceEntityFramework()
    {
        _hub = "Hub";
        _group = "Groupe1";
        _connectionId = Guid.NewGuid().ToString();
        _userId = "user";
        
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
            .AddJsonFile("appsettings.local.json", true)
            .Build();

        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<IConfiguration>(configuration);
            
        serviceCollection.AddDbContext<PubSubDbContext>(options =>
        {
            options.UseInMemoryDatabase("PubSubDbContext");
        });
        
        serviceCollection
            .AddPubSubService(configuration)
            .AddPubSubEntityFrameworkStorage<PubSubDbContext>();

        serviceCollection.AddScoped<IAmazonApiGatewayManagementApi, FakeApiGateway>();
        serviceCollection.AddScoped<IAmazonApiGatewayManagementApiClientProvider, FakeApiGatewayProvider>();
        
        _serviceProvider = serviceCollection.BuildServiceProvider();
        
        _pubSubService = _serviceProvider.GetRequiredService<IPubSubService>();
        
        _pubSubService.ConnectAsync(_hub, _connectionId, _userId, 60);
    }

    private FakeApiGateway GetApiGateway()
    {
        var gatewayProvider = _serviceProvider.GetRequiredService<IAmazonApiGatewayManagementApiClientProvider>();
        var apiGateWay = gatewayProvider.Create(string.Empty, string.Empty);

        return (apiGateWay as FakeApiGateway)!;
    }
    
    [Fact]
    public async Task ShouldCloseConnectionAsync()
    {
        var gateway = GetApiGateway();
        
        await _pubSubService.CloseConnectionAsync(_hub, _connectionId);
        
        Assert.True(gateway.DeleteRequestResponse.Single().Item1.ConnectionId == _connectionId);
        
        Assert.False(await _pubSubService.ConnectionExistsAsync(_hub, _connectionId));
        
        Assert.True(gateway.GetRequestResponse.Single().Item1.ConnectionId == _connectionId);
        
    }
    
    [Fact]
    public async Task ShouldCloseConnectionWhenErrorOccuredAsync()
    {
        var gateway = GetApiGateway();
        gateway.HttpStatusCode = HttpStatusCode.InternalServerError;
        
        await _pubSubService.CloseConnectionAsync(_hub, _connectionId);
        
        Assert.Contains(gateway.DeleteRequestResponse, c => c.Item1.ConnectionId == _connectionId);
        
        Assert.False(await _pubSubService.ConnectionExistsAsync(_hub, _connectionId));
    }

    [Fact]
    public async Task ShouldCloseGroupConnectionAsync()
    {
        var gateway = GetApiGateway();
        
        await _pubSubService.AddConnectionToGroupAsync(_hub, _group, _connectionId);

        Assert.True(await _pubSubService.IsConnectionInGroupAsync(_hub, _group, _connectionId));
        
        await _pubSubService.CloseGroupConnectionsAsync(_hub, _group);
        
        Assert.True(gateway.DeleteRequestResponse.Single().Item1.ConnectionId == _connectionId);
        
        Assert.False(await _pubSubService.ConnectionExistsAsync(_hub, _connectionId));
    }
    
    [Fact]
    public async Task ShouldCloseUserConnectionAsync()
    {
        var gateway = GetApiGateway();
        
        await _pubSubService.AddUserToGroupAsync(_hub, _group, _userId);

        await _pubSubService.GroupExistsAsync(_hub, _group);
        
        Assert.True(await _pubSubService.IsConnectionInGroupAsync(_hub, _group, _connectionId));
        
        await _pubSubService.CloseUserConnectionsAsync(_hub, _userId);
        
        Assert.True(gateway.DeleteRequestResponse.Single().Item1.ConnectionId == _connectionId);
        
        Assert.False(await _pubSubService.ConnectionExistsAsync(_hub, _connectionId));
    }
    
    [Fact]
    public async Task ShouldCloseAllConnectionAsync()
    {
        var gateway = GetApiGateway();
        
        await _pubSubService.CloseAllConnectionsAsync(_hub);
        
        Assert.True(gateway.DeleteRequestResponse.Single().Item1.ConnectionId == _connectionId);
        
        Assert.False(await _pubSubService.ConnectionExistsAsync(_hub, _connectionId));
    }
    
    [Fact]
    public async Task ShouldAddConnectionToGroupAsync()
    {
        Assert.True(await _pubSubService.ConnectionExistsAsync(_hub, _connectionId));
        Assert.True(await _pubSubService.UserExistsAsync(_hub, _userId));
        
        await _pubSubService.AddConnectionToGroupAsync(_hub, _group, _connectionId);

        Assert.True(await _pubSubService.IsConnectionInGroupAsync(_hub, _group, _connectionId));
        
        await _pubSubService.GrantPermissionAsync(_hub, PubSubPermission.SendMessageToGroup, _connectionId);
        
        Assert.True(await _pubSubService.CheckPermissionAsync(_hub, PubSubPermission.SendMessageToGroup, _connectionId, _group));
        
        await _pubSubService.RevokePermissionAsync(_hub, PubSubPermission.SendMessageToGroup, _connectionId);
        
        Assert.False(await _pubSubService.CheckPermissionAsync(_hub, PubSubPermission.SendMessageToGroup, _connectionId, _group));

        await _pubSubService.RemoveConnectionFromGroupAsync(_hub, _group, _connectionId);
        
        Assert.False(await _pubSubService.IsConnectionInGroupAsync(_hub, _group, _connectionId));
    }
    
    
    [Fact]
    public async Task ShouldAddUserToGroupAsync()
    {
        await _pubSubService.AddUserToGroupAsync(_hub, _group, _userId);

        await _pubSubService.GroupExistsAsync(_hub, _group);
        Assert.True(await _pubSubService.IsConnectionInGroupAsync(_hub, _group, _connectionId));
        
        
        await _pubSubService.ConnectAsync(_hub, "other_connection", _userId);
        Assert.True(await _pubSubService.IsConnectionInGroupAsync(_hub, _group, "other_connection"));
       

        await _pubSubService.RemoveUserFromGroupAsync(_hub, _group, _userId);
        
        Assert.False(await _pubSubService.IsConnectionInGroupAsync(_hub, _group, _connectionId));
        
        await _pubSubService.AddUserToGroupAsync(_hub, _group, _userId);
        
        Assert.True(await _pubSubService.IsUserInGroupAsync(_hub, _group, _userId));
        Assert.True(await _pubSubService.IsConnectionInGroupAsync(_hub, _group, _connectionId));
        
        await _pubSubService.RemoveUserFromAllGroupsAsync(_hub,  _userId);
        Assert.False(await _pubSubService.IsConnectionInGroupAsync(_hub, _group, _connectionId));
    }

    [Fact]
    public async Task ShouldCreateAckAsync()
    {
        var ack = Guid.NewGuid().ToString();
        Assert.True(await _pubSubService.CreateAckAsync(_hub, _connectionId, ack));

        Assert.False(await _pubSubService.CreateAckAsync(_hub, _connectionId, ack));
        
        Assert.True(await _pubSubService.CreateAckAsync(_hub, _connectionId, "$"));
    }

    [Fact]
    public async Task ShouldSendToConnectionAsync()
    {
        var message = new PubSubMessage
        {
            ackId = null,
            type = "message"
        };

        await _pubSubService.SendToConnectionAsync(_hub, _connectionId, message);
        
        var gateway = GetApiGateway();
        
        Assert.True(gateway.PostRequestResponse.Single().Item1.ConnectionId == _connectionId);
    }
    
    [Fact]
    public async Task ShouldFailSendToConnection()
    {
        var message = new PubSubMessage
        {
            ackId = null,
            type = "message"
        };

        var gateway = GetApiGateway();
        gateway.HttpStatusCode = HttpStatusCode.Gone;
        
        await _pubSubService.SendToConnectionAsync(_hub, _connectionId, message);
        
        Assert.False(await _pubSubService.ConnectionExistsAsync(_hub, _connectionId));
    }
    
    [Fact]
    public async Task ShouldFailSendToConnectionGoneException()
    {
        var message = new PubSubMessage
        {
            ackId = null,
            type = "message"
        };

        var gateway = GetApiGateway();
        gateway.ThrowGoneException = true;
        
        await _pubSubService.SendToConnectionAsync(_hub, _connectionId, message);
        
        Assert.False(await _pubSubService.ConnectionExistsAsync(_hub, _connectionId));
    }
    
    [Fact]
    public async Task ShouldSendToGroupAsync()
    {
        var message = new PubSubMessage
        {
            ackId = null,
            type = "message",
            @group = _group
        };

        await _pubSubService.AddConnectionToGroupAsync(_hub, _group, _connectionId);
        
        await _pubSubService.SendToGroupAsync(_hub, _group, message);
        
        var gateway = GetApiGateway();
        
        Assert.True(gateway.PostRequestResponse.Single().Item1.ConnectionId == _connectionId);
    }
    
    [Fact]
    public async Task ShouldSendToGroupConnectionExcluded()
    {
        var message = new PubSubMessage
        {
            ackId = null,
            type = "message",
            @group = _group
        };

        await _pubSubService.AddConnectionToGroupAsync(_hub, _group, _connectionId);
        
        await _pubSubService.SendToGroupAsync(_hub, _group, message, new List<string> { _connectionId});
        
        var gateway = GetApiGateway();
        
        Assert.Empty(gateway.PostRequestResponse);
    }
    
    [Fact]
    public async Task ShouldNotSendToGroupNoConnectionsAsync()
    {
        var message = new PubSubMessage
        {
            ackId = null,
            type = "message",
            @group = _group
        };
        
        await _pubSubService.SendToGroupAsync(_hub, _group, message);
        
        var gateway = GetApiGateway();
        
        Assert.Empty(gateway.PostRequestResponse);
    }
    
    [Fact]
    public async Task ShouldSendAckToConnection()
    {
        await _pubSubService.SendAckToConnectionAsync(_hub, _connectionId, "1", true);
        
        var gateway = GetApiGateway();
        
        Assert.True(gateway.PostRequestResponse.Single().Item1.ConnectionId == _connectionId);
    }
    
    [Fact]
    public async Task ShouldSendToUserAsync()
    {
        var message = new PubSubMessage
        {
            ackId = null,
            type = "message"
        };

        
        await _pubSubService.SendToUserAsync(_hub, _userId, message);
        
        var gateway = GetApiGateway();
        
        Assert.True(gateway.PostRequestResponse.Single().Item1.ConnectionId == _connectionId);
    }
    
    [Fact]
    public async Task ShouldSendToUserConnectionExcludedAsync()
    {
        var message = new PubSubMessage
        {
            ackId = null,
            type = "message"
        };

        await _pubSubService.SendToUserAsync(_hub, _userId, message, new List<string> {_connectionId});
        
        var gateway = GetApiGateway();
        
        Assert.Empty(gateway.PostRequestResponse);
    }
    
    [Fact]
    public async Task ShouldSendToAll()
    {
        var message = new PubSubMessage
        {
            ackId = null,
            type = "message"
        };

        
        await _pubSubService.SendToAllAsync(_hub, message);
        
        var gateway = GetApiGateway();
        
        Assert.True(gateway.PostRequestResponse.Single().Item1.ConnectionId == _connectionId);
    }
    
    [Fact]
    public async Task ShouldSendToAllConnectionExcludedAsync()
    {
        var message = new PubSubMessage
        {
            ackId = null,
            type = "message"
        };

        await _pubSubService.SendToAllAsync(_hub,  message, new List<string> {_connectionId});
        
        var gateway = GetApiGateway();
        
        Assert.Empty(gateway.PostRequestResponse);
    }

    [Fact]
    public void ShouldGenerateToken()
    {
        var secret = Guid.NewGuid().ToString();
        const string issuer = "issuer";
        const string audience = "audience";
        
        var token = _pubSubService.GenerateToken(issuer, audience, _userId, new List<string> { "role"},
            secret);
        
        var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
        var keys = new List<SecurityKey> { mySecurityKey };
        
        new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = keys,
            ValidIssuers = new List<string>{issuer},
            ValidateIssuer = true,
            ValidAudiences = new List<string>{audience},
            ValidateAudience = true
        }, out _);
    }
}