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
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Nuages.MongoDB;
using Nuages.MongoDB.DatabaseProvider;
using Nuages.PubSub.Storage.InMemory.DataModel;
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
    private readonly IMongoClient _client;
    private readonly string _dbName;

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
            .AddPubSubMongoStorage();

        serviceCollection.AddScoped<IAmazonApiGatewayManagementApi, FakeApiGateway>();
        serviceCollection.AddScoped<IAmazonApiGatewayManagementApiClientProvider, FakeApiGatewayProvider>();
        
        _dbName = configuration.GetSection("Nuages:DbName").Value + "-" + Guid.NewGuid().ToString().Split("-").First() ;

        serviceCollection.AddSingleton<IMongoDatabaseNameProvider>(new TestDbNameProvider(_dbName));
        
        _serviceProvider = serviceCollection.BuildServiceProvider();
        
        _pubSubService = _serviceProvider.GetRequiredService<IPubSubService>();
        
        var clientProvider = _serviceProvider.GetRequiredService<IMongoClientProvider>();
        _client = clientProvider.CreateClient<PubSubConnection>();
        
        _pubSubService.ConnectAsync(_hub, _connectionId, _sub);
        
        System.Threading.Thread.Sleep(200);
    }
    
    FakeApiGateway GetApiGateway()
    {
        var gatewayProvider = _serviceProvider.GetRequiredService<IAmazonApiGatewayManagementApiClientProvider>();
        var apiGateWay = gatewayProvider.Create(string.Empty, string.Empty);

        return (apiGateWay as FakeApiGateway)!;
    }
    
    [Fact]
    [TestPriority(0)]
    public async Task ShouldCloseConnection()
    {
        //var gateway = GetApiGateway();
        
        await _pubSubService.CloseConnectionAsync(_hub, _connectionId);
        
        //Assert.True(gateway.DeleteRequestResponse.Single().Item1.ConnectionId == _connectionId);
        
        Assert.False(await _pubSubService.ConnectionExistsAsync(_hub, _connectionId));
    }
    
    [Fact]
    [TestPriority(1)]
    public async Task ShouldCloseConnectionWhenErrorOccured()
    {
        var gateway = GetApiGateway();
        gateway.HttpStatusCode = HttpStatusCode.InternalServerError;
        
        await _pubSubService.CloseConnectionAsync(_hub, _connectionId);
        
        Assert.True(gateway.DeleteRequestResponse.Count( c => c.Item1.ConnectionId == _connectionId) == 0);
        
        Assert.False(await _pubSubService.ConnectionExistsAsync(_hub, _connectionId));
    }

    [Fact]
    [TestPriority(2)]
    public async Task ShouldCloseGroupConnection()
    {
        var gateway = GetApiGateway();
        
        await _pubSubService.AddConnectionToGroupAsync(_hub, _group, _connectionId, _sub);

        Assert.True(await _pubSubService.IsConnectionInGroup(_hub, _group, _connectionId));
        
        await _pubSubService.CloseGroupConnectionsAsync(_hub, _group);
        
        Assert.True(gateway.DeleteRequestResponse.Single().Item1.ConnectionId == _connectionId);
        
        Assert.False(await _pubSubService.ConnectionExistsAsync(_hub, _connectionId));
    }
    
    [Fact]
    [TestPriority(3)]
    public async Task ShouldCloseUserConnection()
    {
        var gateway = GetApiGateway();
        
        await _pubSubService.AddUserToGroupAsync(_hub, _group, _sub);

        await _pubSubService.GroupExistsAsync(_hub, _group);
        
        Assert.True(await _pubSubService.IsConnectionInGroup(_hub, _group, _connectionId));
        
        await _pubSubService.CloseUserConnectionsAsync(_hub, _sub);
        
        Assert.True(gateway.DeleteRequestResponse.Single().Item1.ConnectionId == _connectionId);
        
        Assert.False(await _pubSubService.ConnectionExistsAsync(_hub, _connectionId));
    }
    
    [Fact]
    [TestPriority(4)]
    public async Task ShouldCloseAllConnection()
    {
        //var gateway = GetApiGateway();
        
        await _pubSubService.CloseAllConnectionsAsync(_hub);
        
        //Assert.True(gateway.DeleteRequestResponse.Single().Item1.ConnectionId == _connectionId);
        
        Assert.False(await _pubSubService.ConnectionExistsAsync(_hub, _connectionId));
    }
    
    [Fact]
    [TestPriority(5)]
    public async Task ShouldAddConnectionToGroup()
    {
        Assert.True(await _pubSubService.ConnectionExistsAsync(_hub, _connectionId));
        
        Assert.True(await _pubSubService.UserExistsAsync(_hub, _sub));
        
        await _pubSubService.AddConnectionToGroupAsync(_hub, _group, _connectionId, _sub);

        Assert.True(await _pubSubService.IsConnectionInGroup(_hub, _group, _connectionId));
        
        await _pubSubService.GrantPermissionAsync(_hub, PubSubPermission.SendMessageToGroup, _connectionId);
        
        Assert.True(await _pubSubService.CheckPermissionAsync(_hub, PubSubPermission.SendMessageToGroup, _connectionId, _group));
        
        await _pubSubService.RevokePermissionAsync(_hub, PubSubPermission.SendMessageToGroup, _connectionId);
        
        Assert.False(await _pubSubService.CheckPermissionAsync(_hub, PubSubPermission.SendMessageToGroup, _connectionId, _group));

        await _pubSubService.RemoveConnectionFromGroupAsync(_hub, _group, _connectionId);
        
        Assert.False(await _pubSubService.IsConnectionInGroup(_hub, _group, _connectionId));
    }
    
    
    [Fact]
    [TestPriority(6)]
    public async Task ShouldAddUserToGroup()
    {
        await _pubSubService.AddUserToGroupAsync(_hub, _group, _sub);

        await _pubSubService.GroupExistsAsync(_hub, _group);
        Assert.True(await _pubSubService.IsConnectionInGroup(_hub, _group, _connectionId));
        
        
        await _pubSubService.ConnectAsync(_hub, "other_connection", _sub);
        Assert.True(await _pubSubService.IsConnectionInGroup(_hub, _group, "other_connection"));
       

        await _pubSubService.RemoveUserFromGroupAsync(_hub, _group, _sub);
        
        Assert.False(await _pubSubService.IsConnectionInGroup(_hub, _group, _connectionId));
        
        await _pubSubService.AddUserToGroupAsync(_hub, _group, _sub);
        Assert.True(await _pubSubService.IsConnectionInGroup(_hub, _group, _connectionId));
        
        await _pubSubService.RemoveUserFromAllGroupsAsync(_hub,  _sub);
        Assert.False(await _pubSubService.IsConnectionInGroup(_hub, _group, _connectionId));
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
    public async Task ShouldSendToConnection()
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
    [TestPriority(10)]
    public async Task ShouldSendToGroup()
    {
        var message = new PubSubMessage
        {
            ackId = null,
            type = "message",
            group = _group
        };

        await _pubSubService.AddConnectionToGroupAsync(_hub, _group, _connectionId, _sub);
        
        await _pubSubService.SendToGroupAsync(_hub, _group, message);
        
        var gateway = GetApiGateway();
        
        Assert.True(gateway.PostRequestResponse.Single().Item1.ConnectionId == _connectionId);
    }
    
    [Fact]
    [TestPriority(11)]
    public async Task ShouldSendToGroupConnectionExcluded()
    {
        var message = new PubSubMessage
        {
            ackId = null,
            type = "message",
            group = _group
        };

        await _pubSubService.AddConnectionToGroupAsync(_hub, _group, _connectionId, _sub);
        
        await _pubSubService.SendToGroupAsync(_hub, _group, message, new List<string> { _connectionId});
        
        var gateway = GetApiGateway();
        
        Assert.Empty(gateway.PostRequestResponse);
    }
    
    [Fact]
    [TestPriority(12)]
    public async Task ShouldNotSendToGroupNoConnections()
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
    public async Task ShouldSendAckToConnection()
    {
        await _pubSubService.SendAckToConnectionAsync(_hub, _connectionId, "1", true);
        
        var gateway = GetApiGateway();
        
        Assert.True(gateway.PostRequestResponse.Single().Item1.ConnectionId == _connectionId);
    }
    
    [Fact]
    [TestPriority(14)]
    public async Task ShouldSendToUser()
    {
        var message = new PubSubMessage
        {
            ackId = null,
            type = "message"
        };

        
        await _pubSubService.SendToUserAsync(_hub, _sub, message);
        
        // var gateway = GetApiGateway();
        //
        // Assert.True(gateway.PostRequestResponse.Single().Item1.ConnectionId == _connectionId);
    }
    
    [Fact]
    [TestPriority(0)]
    public async Task ShouldSendToUserConnectionExcluded()
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
    public async Task ShouldSendToAll()
    {
        var message = new PubSubMessage
        {
            ackId = null,
            type = "message"
        };

        
        await _pubSubService.SendToAllAsync(_hub, message);
        
        // var gateway = GetApiGateway();
        //
        // Assert.True(gateway.PostRequestResponse.Single().Item1.ConnectionId == _connectionId);
    }
    
    [Fact]
    [TestPriority(16)]
    public async Task ShouldSendToAllConnectionExcluded()
    {
        var message = new PubSubMessage
        {
            ackId = null,
            type = "message"
        };

        await _pubSubService.SendToAllAsync(_hub,  message, new List<string> {_connectionId});
        
        // var gateway = GetApiGateway();
        //
        // Assert.Empty(gateway.PostRequestResponse);
    }

    [Fact]
    [TestPriority(17)]
    public void ShouldGenerateToken()
    {
        var secret = Guid.NewGuid().ToString();
        var issuer = "issuer";
        var audience = "audience";
        
        var token = _pubSubService.GenerateToken(issuer, audience, _sub, new List<string> { "role"},
            secret, null);
        
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
        _client.DropDatabase(_dbName);
        _serviceProvider.Dispose();
    }
}

class TestDbNameProvider : IMongoDatabaseNameProvider
{
    private readonly string _name;

    public TestDbNameProvider(string name)
    {
        _name = name;
    }
    
    public string GetDatabaseName<TDocument>()
    {
        return _name;
    }
}