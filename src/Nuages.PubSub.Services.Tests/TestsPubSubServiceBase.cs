using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Nuages.PubSub.Services.Tests;

public abstract class TestsPubSubServiceBase
{
    protected IPubSubService PubSubService = null!;
    protected string Hub = string.Empty;
    protected string Group = string.Empty;  
    protected string ConnectionId = string.Empty;  
    protected string UserId = string.Empty;  
    protected ServiceProvider ServiceProvider = null!;   

    private FakeApiGateway GetApiGateway()
    {
        var gatewayProvider = ServiceProvider.GetRequiredService<IAmazonApiGatewayManagementApiClientProvider>();
        var apiGateWay = gatewayProvider.Create(string.Empty, string.Empty);

        return (apiGateWay as FakeApiGateway)!;
    }
    
    [Fact]
    public async Task ShouldCloseConnectionAsync()
    {
        var gateway = GetApiGateway();
        
        await PubSubService.CloseConnectionAsync(Hub, ConnectionId);
        
        Assert.True(gateway.DeleteRequestResponse.Single().Item1.ConnectionId == ConnectionId);
        
        Assert.False(await PubSubService.ConnectionExistsAsync(Hub, ConnectionId));
        
        Assert.True(gateway.GetRequestResponse.Single().Item1.ConnectionId == ConnectionId);
        
    }
    
    [Fact]
    public async Task ShouldCloseConnectionWhenErrorOccuredAsync()
    {
        var gateway = GetApiGateway();
        gateway.HttpStatusCode = HttpStatusCode.InternalServerError;
        
        await PubSubService.CloseConnectionAsync(Hub, ConnectionId);
        
        Assert.Contains(gateway.DeleteRequestResponse, c => c.Item1.ConnectionId == ConnectionId);
        
        Assert.False(await PubSubService.ConnectionExistsAsync(Hub, ConnectionId));
    }

    [Fact]
    public async Task ShouldCloseGroupConnectionAsync()
    {
        var gateway = GetApiGateway();
        
        await PubSubService.AddConnectionToGroupAsync(Hub, Group, ConnectionId);

        Assert.True(await PubSubService.IsConnectionInGroupAsync(Hub, Group, ConnectionId));
        
        await PubSubService.CloseGroupConnectionsAsync(Hub, Group);
        
        Assert.True(gateway.DeleteRequestResponse.Single().Item1.ConnectionId == ConnectionId);
        
        Assert.False(await PubSubService.ConnectionExistsAsync(Hub, ConnectionId));
    }
    
    [Fact]
    public async Task ShouldCloseUserConnectionAsync()
    {
        var gateway = GetApiGateway();
        
        await PubSubService.AddUserToGroupAsync(Hub, Group, UserId);

        await PubSubService.GroupExistsAsync(Hub, Group);
        
        Assert.True(await PubSubService.IsConnectionInGroupAsync(Hub, Group, ConnectionId));
        
        await PubSubService.CloseUserConnectionsAsync(Hub, UserId);
        
        Assert.True(gateway.DeleteRequestResponse.Single().Item1.ConnectionId == ConnectionId);
        
        Assert.False(await PubSubService.ConnectionExistsAsync(Hub, ConnectionId));
    }
    
    [Fact]
    public async Task ShouldCloseAllConnectionAsync()
    {
        var gateway = GetApiGateway();
        
        await PubSubService.CloseAllConnectionsAsync(Hub);
        
        Assert.True(gateway.DeleteRequestResponse.Single().Item1.ConnectionId == ConnectionId);
        
        Assert.False(await PubSubService.ConnectionExistsAsync(Hub, ConnectionId));
    }
    
    [Fact]
    public async Task ShouldAddConnectionToGroupAsync()
    {
        
        Assert.True(await PubSubService.ConnectionExistsAsync(Hub, ConnectionId));
        Assert.True(await PubSubService.UserExistsAsync(Hub, UserId));
        
        await PubSubService.AddConnectionToGroupAsync(Hub, Group, ConnectionId);

        Assert.True(await PubSubService.IsConnectionInGroupAsync(Hub, Group, ConnectionId));
        
        await PubSubService.GrantPermissionAsync(Hub, PubSubPermission.SendMessageToGroup, ConnectionId);
        
        Assert.True(await PubSubService.CheckPermissionAsync(Hub, PubSubPermission.SendMessageToGroup, ConnectionId, Group));
        
        await PubSubService.RevokePermissionAsync(Hub, PubSubPermission.SendMessageToGroup, ConnectionId);
        
        Assert.False(await PubSubService.CheckPermissionAsync(Hub, PubSubPermission.SendMessageToGroup, ConnectionId, Group));

        await PubSubService.RemoveConnectionFromGroupAsync(Hub, Group, ConnectionId);
        
        Assert.False(await PubSubService.IsConnectionInGroupAsync(Hub, Group, ConnectionId));
    }
    
    
    [Fact]
    public async Task ShouldAddUserToGroupAsync()
    {
        await PubSubService.AddUserToGroupAsync(Hub, Group, UserId);

        await PubSubService.GroupExistsAsync(Hub, Group);
        Assert.True(await PubSubService.IsConnectionInGroupAsync(Hub, Group, ConnectionId));
        
        
        await PubSubService.ConnectAsync(Hub, "other_connection", UserId);
        Assert.True(await PubSubService.IsConnectionInGroupAsync(Hub, Group, "other_connection"));
       

        await PubSubService.RemoveUserFromGroupAsync(Hub, Group, UserId);
        
        Assert.False(await PubSubService.IsConnectionInGroupAsync(Hub, Group, ConnectionId));

        Group = "Groupe2";
        
        await PubSubService.AddUserToGroupAsync(Hub, Group, UserId);
        
        Assert.True(await PubSubService.IsUserInGroupAsync(Hub, Group, UserId));
        Assert.True(await PubSubService.IsConnectionInGroupAsync(Hub, Group, ConnectionId));
        
        await PubSubService.RemoveUserFromAllGroupsAsync(Hub,  UserId);
        Assert.False(await PubSubService.IsConnectionInGroupAsync(Hub, Group, ConnectionId));
    }

    [Fact]
    public async Task ShouldCreateAckAsync()
    {
        var ack = Guid.NewGuid().ToString();
        Assert.True(await PubSubService.CreateAckAsync(Hub, ConnectionId, ack));

        Assert.False(await PubSubService.CreateAckAsync(Hub, ConnectionId, ack));
        
        Assert.True(await PubSubService.CreateAckAsync(Hub, ConnectionId, "$"));
    }

    [Fact]
    public async Task ShouldSendToConnectionAsync()
    {
        var message = new PubSubMessage
        {
            ackId = null,
            type = "message"
        };

        await PubSubService.SendToConnectionAsync(Hub, ConnectionId, message);
        
        var gateway = GetApiGateway();
        
        Assert.True(gateway.PostRequestResponse.Single().Item1.ConnectionId == ConnectionId);
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
        
        await PubSubService.SendToConnectionAsync(Hub, ConnectionId, message);
        
        Assert.False(await PubSubService.ConnectionExistsAsync(Hub, ConnectionId));
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
        
        await PubSubService.SendToConnectionAsync(Hub, ConnectionId, message);
        
        Assert.False(await PubSubService.ConnectionExistsAsync(Hub, ConnectionId));
    }
    
    [Fact]
    public async Task ShouldSendToGroupAsync()
    {
        var message = new PubSubMessage
        {
            ackId = null,
            type = "message",
            group = Group
        };

        await PubSubService.AddConnectionToGroupAsync(Hub, Group, ConnectionId);
        
        await PubSubService.SendToGroupAsync(Hub, Group, message);
        
        var gateway = GetApiGateway();
        
        Assert.True(gateway.PostRequestResponse.Single().Item1.ConnectionId == ConnectionId);
    }
    
    [Fact]
    public async Task ShouldSendToGroupConnectionExcluded()
    {
        var message = new PubSubMessage
        {
            ackId = null,
            type = "message",
            group = Group
        };

        await PubSubService.AddConnectionToGroupAsync(Hub, Group, ConnectionId);
        
        await PubSubService.SendToGroupAsync(Hub, Group, message, new List<string> { ConnectionId});
        
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
            group = Group
        };
        
        await PubSubService.SendToGroupAsync(Hub, Group, message);
        
        var gateway = GetApiGateway();
        
        Assert.Empty(gateway.PostRequestResponse);
    }
    
    [Fact]
    public async Task ShouldSendAckToConnection()
    {
        await PubSubService.SendAckToConnectionAsync(Hub, ConnectionId, "1", true);
        
        var gateway = GetApiGateway();
        
        Assert.True(gateway.PostRequestResponse.Single().Item1.ConnectionId == ConnectionId);
    }
    
    [Fact]
    public async Task ShouldSendToUserAsync()
    {
        var message = new PubSubMessage
        {
            ackId = null,
            type = "message"
        };
        
        await PubSubService.SendToUserAsync(Hub, UserId, message);
        
        var gateway = GetApiGateway();
        
        Assert.True(gateway.PostRequestResponse.Single().Item1.ConnectionId == ConnectionId);
    }
    
    [Fact]
    public async Task ShouldSendToUserConnectionExcludedAsync()
    {
        var message = new PubSubMessage
        {
            ackId = null,
            type = "message"
        };

        await PubSubService.SendToUserAsync(Hub, UserId, message, new List<string> {ConnectionId});
        
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
        
        await PubSubService.SendToAllAsync(Hub, message);
        
        var gateway = GetApiGateway();
        
        Assert.True(gateway.PostRequestResponse.Single().Item1.ConnectionId == ConnectionId);
    }
    
    [Fact]
    public async Task ShouldSendToAllConnectionExcludedAsync()
    {
        var message = new PubSubMessage
        {
            ackId = null,
            type = "message"
        };

        await PubSubService.SendToAllAsync(Hub,  message, new List<string> {ConnectionId});
        
        var gateway = GetApiGateway();
        
        Assert.Empty(gateway.PostRequestResponse);
    }

    [Fact]
    public void ShouldGenerateToken()
    {
        var secret = Guid.NewGuid().ToString();
        const string issuer = "issuer";
        const string audience = "audience";
        
        var token = PubSubService.GenerateToken(issuer, audience, UserId, new List<string> { "role"},
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