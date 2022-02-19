using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Amazon.ApiGatewayManagementApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Nuages.PubSub.Storage.Mongo;
using Xunit;


namespace Nuages.PubSub.Services.Tests;

[TestCaseOrderer("Nuages.API.Tests.Infrastructure.PriorityOrderer", "Nuages.PubSub.Services.Tests")]
public class TestsPubSubServiceMongo : IDisposable
{
    private readonly IPubSubService _pubSubService;
    private readonly string _hub;
    private readonly string _group;
    private readonly string _connectionId;
    private readonly string _sub;
    private readonly ServiceProvider _serviceProvider;
    private readonly string _dbName;
    private readonly MongoClient _client;

    public TestsPubSubServiceMongo()
    {
        _hub = "Hub";
        _group = "Groupe1";
        _connectionId = Guid.NewGuid().ToString();
        _sub = "user";
        
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
            .AddJsonFile("appsettings.local.json", false)
            .Build();

        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<IConfiguration>(configuration);
            
        serviceCollection
            .AddPubSubService(configuration)
            .AddPubSubMongoStorage(config =>
            {
                config.ConnectionString = configuration["Nuages:Mongo:ConnectionString"];
                config.DatabaseName = configuration["Nuages:Mongo:DatabaseName"];
            });

        serviceCollection.AddScoped<IAmazonApiGatewayManagementApi, FakeApiGateway>();
        serviceCollection.AddScoped<IAmazonApiGatewayManagementApiClientProvider, FakeApiGatewayProvider>();
        _serviceProvider = serviceCollection.BuildServiceProvider();

        var options = _serviceProvider.GetRequiredService<IOptions<PubSubMongoOptions>>().Value;
        
        var connectionString = options.ConnectionString;
        _dbName = options.DatabaseName;
        
        _client = new MongoClient(connectionString);
        
        _client.DropDatabase(_dbName);
        
        _pubSubService = _serviceProvider.GetRequiredService<IPubSubService>();

        _pubSubService.ConnectAsync(_hub, _connectionId, _sub);
        
        System.Threading.Thread.Sleep(200);
    }

    private FakeApiGateway GetApiGateway()
    {
        var gatewayProvider = _serviceProvider.GetRequiredService<IAmazonApiGatewayManagementApiClientProvider>();
        var apiGateWay = gatewayProvider.Create(string.Empty, string.Empty);

        return (apiGateWay as FakeApiGateway)!;
    }
    
    [Fact]
    [TestPriority(0)]
    public async Task ShouldCloseConnectionAsync()
    {
        var gateway = GetApiGateway();
        
        await _pubSubService.CloseConnectionAsync(_hub, _connectionId);
        
        Assert.True(gateway.DeleteRequestResponse.Single().Item1.ConnectionId == _connectionId);
        
        Assert.False(await _pubSubService.ConnectionExistsAsync(_hub, _connectionId));
    }
    
    [Fact]
    [TestPriority(1)]
    public async Task ShouldCloseConnectionWhenErrorOccuredAsync()
    {
        var gateway = GetApiGateway();
        gateway.HttpStatusCode = HttpStatusCode.InternalServerError;
        
        await _pubSubService.CloseConnectionAsync(_hub, _connectionId);
        
        Assert.Contains(gateway.DeleteRequestResponse, c => c.Item1.ConnectionId == _connectionId);
        
        Assert.False(await _pubSubService.ConnectionExistsAsync(_hub, _connectionId));
    }

    [Fact]
    [TestPriority(2)]
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
    [TestPriority(3)]
    public async Task ShouldCloseUserConnectionAsync()
    {
        var gateway = GetApiGateway();
        
        await _pubSubService.AddUserToGroupAsync(_hub, _group, _sub);

        await _pubSubService.GroupExistsAsync(_hub, _group);
        
        Assert.True(await _pubSubService.IsConnectionInGroupAsync(_hub, _group, _connectionId));
        
        await _pubSubService.CloseUserConnectionsAsync(_hub, _sub);
        
        Assert.True(gateway.DeleteRequestResponse.Single().Item1.ConnectionId == _connectionId);
        
        Assert.False(await _pubSubService.ConnectionExistsAsync(_hub, _connectionId));
    }
    
    [Fact]
    [TestPriority(4)]
    public async Task ShouldCloseAllConnectionAsync()
    {
        var gateway = GetApiGateway();
        
        await _pubSubService.CloseAllConnectionsAsync(_hub);
        
        Assert.True(gateway.DeleteRequestResponse.Single().Item1.ConnectionId == _connectionId);
        
        Assert.False(await _pubSubService.ConnectionExistsAsync(_hub, _connectionId));
    }
    
    [Fact]
    [TestPriority(5)]
    public async Task ShouldAddConnectionToGroupAsync()
    {
        Assert.True(await _pubSubService.ConnectionExistsAsync(_hub, _connectionId));
        
        Assert.True(await _pubSubService.UserExistsAsync(_hub, _sub));
        
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
    [TestPriority(6)]
    public async Task ShouldAddUserToGroupAsync()
    {
        await _pubSubService.AddUserToGroupAsync(_hub, _group, _sub);

        await _pubSubService.GroupExistsAsync(_hub, _group);
        Assert.True(await _pubSubService.IsConnectionInGroupAsync(_hub, _group, _connectionId));
        
        
        await _pubSubService.ConnectAsync(_hub, "other_connection", _sub);
        Assert.True(await _pubSubService.IsConnectionInGroupAsync(_hub, _group, "other_connection"));
       

        await _pubSubService.RemoveUserFromGroupAsync(_hub, _group, _sub);
        
        Assert.False(await _pubSubService.IsConnectionInGroupAsync(_hub, _group, _connectionId));
        
        await _pubSubService.AddUserToGroupAsync(_hub, _group, _sub);
        Assert.True(await _pubSubService.IsConnectionInGroupAsync(_hub, _group, _connectionId));
        
        await _pubSubService.RemoveUserFromAllGroupsAsync(_hub,  _sub);
        Assert.False(await _pubSubService.IsConnectionInGroupAsync(_hub, _group, _connectionId));
    }

    [Fact]
    [TestPriority(7)]
    public async Task ShouldCreateAck()
    {
        var ack = Guid.NewGuid().ToString();
        Assert.True(await _pubSubService.CreateAckAsync(_hub, _connectionId, ack));

        Assert.False(await _pubSubService.CreateAckAsync(_hub, _connectionId, ack));
        
        Assert.True(await _pubSubService.CreateAckAsync(_hub, _connectionId, "$"));
    }

    [Fact]
    [TestPriority(8)]
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
    [TestPriority(9)]
    public async Task ShouldFailSendToConnectionAsync()
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
    [TestPriority(9)]
    public async Task ShouldFailSendToConnectionAsyncGoneEsxception()
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
    [TestPriority(10)]
    public async Task ShouldSendToGroupAsync()
    {
        var message = new PubSubMessage
        {
            ackId = null,
            type = "message",
            group = _group
        };

        await _pubSubService.AddConnectionToGroupAsync(_hub, _group, _connectionId);
        
        await _pubSubService.SendToGroupAsync(_hub, _group, message);
        
        var gateway = GetApiGateway();
        
        Assert.True(gateway.PostRequestResponse.Single().Item1.ConnectionId == _connectionId);
    }
    
    [Fact]
    [TestPriority(11)]
    public async Task ShouldSendToGroupConnectionExcludedAsync()
    {
        var message = new PubSubMessage
        {
            ackId = null,
            type = "message",
            group = _group
        };

        await _pubSubService.AddConnectionToGroupAsync(_hub, _group, _connectionId);
        
        await _pubSubService.SendToGroupAsync(_hub, _group, message, new List<string> { _connectionId});
        
        var gateway = GetApiGateway();
        
        Assert.Empty(gateway.PostRequestResponse);
    }
    
    [Fact]
    [TestPriority(12)]
    public async Task ShouldNotSendToGroupNoConnectionsAsync()
    {
        var message = new PubSubMessage
        {
            ackId = null,
            type = "message",
            group = _group
        };
        
        await _pubSubService.SendToGroupAsync(_hub, _group, message);
        
        var gateway = GetApiGateway();
        
        Assert.Empty(gateway.PostRequestResponse);
    }
    
    [Fact]
    [TestPriority(13)]
    public async Task ShouldSendAckToConnectionAsync()
    {
        await _pubSubService.SendAckToConnectionAsync(_hub, _connectionId, "1", true);
        
        var gateway = GetApiGateway();
        
        Assert.True(gateway.PostRequestResponse.Single().Item1.ConnectionId == _connectionId);
    }
    
    [Fact]
    [TestPriority(14)]
    public async Task ShouldSendToUserAsync()
    {
        var message = new PubSubMessage
        {
            ackId = null,
            type = "message"
        };

        
        await _pubSubService.SendToUserAsync(_hub, _sub, message);
        
        var gateway = GetApiGateway();
        
        Assert.True(gateway.PostRequestResponse.Single().Item1.ConnectionId == _connectionId);
    }
    
    [Fact]
    [TestPriority(0)]
    public async Task ShouldSendToUserConnectionExcludedAsync()
    {
        var message = new PubSubMessage
        {
            ackId = null,
            type = "message"
        };

        await _pubSubService.SendToUserAsync(_hub, _sub, message, new List<string> {_connectionId});
        
        var gateway = GetApiGateway();
        
        Assert.Empty(gateway.PostRequestResponse);
    }
    
    [Fact]
    [TestPriority(15)]
    public async Task ShouldSendToAllAsync()
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
    [TestPriority(16)]
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
    [TestPriority(17)]
    public void ShouldGenerateToken()
    {
        var secret = Guid.NewGuid().ToString();
        const string issuer = "issuer";
        const string audience = "audience";
        
        var token = _pubSubService.GenerateToken(issuer, audience, _sub, new List<string> { "role"},
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

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        
        _client.DropDatabase(_dbName);
        _serviceProvider.Dispose();
    }
}
