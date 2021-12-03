using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Nuages.PubSub.Services;
using Xunit;

namespace Nuages.PubSub.WebSocket.Tests;

public class TestPubSubFunction
{
    public TestPubSubFunction()
    {

    }
    
    [Fact]
    public async Task TestConnect()
    {
        var pubSubService = new Mock<IPubSubService>();
        
        var function = new CustomPubSubFunction(pubSubService.Object);
        
        var lambdaContext = new TestLambdaContext();
        
        var connectionId = "test-id";
        
        var request = new APIGatewayProxyRequest
        {
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                ConnectionId = connectionId,
                Authorizer = new APIGatewayCustomAuthorizerContext
                {
                    
                }
            }
        };

        var hub = "hub";
        var sub = "user";
        
        request.RequestContext.Authorizer["sub"] = sub;
        request.RequestContext.Authorizer["nuageshub"] = hub;
        request.RequestContext.Authorizer["roles"] = "JoinOrLeaveGroup";
        
        //Ok
        var res = await function.OnConnectHandlerAsync(request, lambdaContext);

        Assert.Equal(200, res.StatusCode);
        
        //Throw
        pubSubService.Setup(c => c.ConnectAsync(hub, connectionId, sub, It.IsAny<TimeSpan?>())).ThrowsAsync(new Exception());

        res = await function.OnConnectHandlerAsync(request, lambdaContext);
        
        Assert.Equal(500, res.StatusCode);
    }
    
    [Fact]
    public async Task TestDisconnect()
    {
        var pubSubService = new Mock<IPubSubService>();
        
        var function = new CustomPubSubFunction(pubSubService.Object);
        
        var lambdaContext = new TestLambdaContext();
        
        var connectionId = "test-id";
        
        var request = new APIGatewayProxyRequest
        {
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                ConnectionId = connectionId,
                Authorizer = new APIGatewayCustomAuthorizerContext
                {
                    
                }
            }
        };

        var hub = "hub";
        var sub = "user";
        
        request.RequestContext.Authorizer["sub"] = sub;
        request.RequestContext.Authorizer["nuageshub"] = hub;
        
        //Ok
        var res = await function.OnDisconnectHandlerAsync(request, lambdaContext);
        Assert.Equal(200, res.StatusCode);
        
        
        //Throw
        pubSubService.Setup(c => c.CloseConnectionAsync(hub, connectionId)).ThrowsAsync(new Exception());
        res = await function.OnDisconnectHandlerAsync(request, lambdaContext);
        
        Assert.Equal(500, res.StatusCode);
    }
    
    [Fact]
    public async Task TestEcho()
    {
        var pubSubService = new Mock<IPubSubService>();
        
        var function = new CustomPubSubFunction(pubSubService.Object);
        
        var lambdaContext = new TestLambdaContext();
        
        string connectionId = "test-id";
        
        var request = new APIGatewayProxyRequest
        {
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                ConnectionId = connectionId,
                Authorizer = new APIGatewayCustomAuthorizerContext
                {
                    ["sub"] = "user",
                    ["nuageshub"] = "hub"
                }
            }
        };

        
        //Ok
        pubSubService.Setup(c => c.SendToConnectionAsync(It.IsAny<string>(), connectionId, It.IsAny<PubSubMessage>())).ReturnsAsync(
        new APIGatewayProxyResponse
        {
            StatusCode = 200
        });

        var  res = await function.EchoHandlerAsync(request, lambdaContext);
        Assert.Equal(200, res.StatusCode);
        
        //Throw
        pubSubService.Setup(c => c.SendToConnectionAsync(It.IsAny<string>(), connectionId, It.IsAny<PubSubMessage>())).ThrowsAsync(new Exception());

        res = await function.EchoHandlerAsync(request, lambdaContext);
        
        Assert.Equal(500, res.StatusCode);
    }
    
    [Fact]
    public async Task TestJoin()
    {
        var pubSubService = new Mock<IPubSubService>();
        
        var function = new CustomPubSubFunction(pubSubService.Object);
        
        var lambdaContext = new TestLambdaContext();
        
        var connectionId = "test-id";
        var hub = "hub";
        var group = "group";
        var user = "user";

        var body = new
        {
            type = "join",
            dataType = "json",
            group,
            ackId = "$"
        };
        
        var request = new APIGatewayProxyRequest
        {
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                ConnectionId = connectionId,
                Authorizer = new APIGatewayCustomAuthorizerContext
                {
                    ["sub"] = user,
                    ["nuageshub"] = hub
                }
                
            },
            Body = JsonSerializer.Serialize(body)
        };

        pubSubService.Setup(c => c.CreateAckAsync(hub, connectionId, It.IsAny<string?>())).ReturnsAsync(true);
        pubSubService.Setup(c => c.CheckPermissionAsync(hub, PubSubPermission.JoinOrLeaveGroup, connectionId, group))
            .ReturnsAsync(true);
        
        
        //ok
        var res = await function.JoinHandlerAsync(request, lambdaContext);
        Assert.Equal(200, res.StatusCode);
        
       
        //Throw
        
        pubSubService.Setup(c => c.AddConnectionToGroupAsync(hub, group, connectionId, user)).ThrowsAsync(new Exception());
        res = await function.JoinHandlerAsync(request, lambdaContext);
        Assert.Equal(500, res.StatusCode);
        
        //Not authorized
        pubSubService.Setup(c => c.CheckPermissionAsync(hub, PubSubPermission.JoinOrLeaveGroup, connectionId, group))
            .ReturnsAsync(false);
        
        res = await function.JoinHandlerAsync(request, lambdaContext);
        Assert.Equal(403, res.StatusCode);
        
        //Ack not valid
        pubSubService.Setup(c => c.CreateAckAsync(hub, connectionId, It.IsAny<string?>())).ReturnsAsync(false);
        
        res = await function.JoinHandlerAsync(request, lambdaContext);
        Assert.Equal(400, res.StatusCode);
    }
    
    [Fact]
    public async Task TestLeave()
    {
        var pubSubService = new Mock<IPubSubService>();
        
        var function = new CustomPubSubFunction(pubSubService.Object);
        
        var lambdaContext = new TestLambdaContext();
        
        var connectionId = "test-id";
        var hub = "hub";
        var group = "group";
        var user = "user";
        
        var body = new
        {
            type = "join",
            dataType = "json",
            group,
            ackId = "$"
        };
        
        var request = new APIGatewayProxyRequest
        {
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                ConnectionId = connectionId,
                Authorizer = new APIGatewayCustomAuthorizerContext
                {
                    ["sub"] = user,
                    ["nuageshub"] = hub
                }
            },
            Body = JsonSerializer.Serialize(body)
        };

        pubSubService.Setup(c => c.CreateAckAsync(hub, connectionId, It.IsAny<string?>())).ReturnsAsync(true);
        pubSubService.Setup(c => c.CheckPermissionAsync(hub, PubSubPermission.JoinOrLeaveGroup, connectionId, group))
            .ReturnsAsync(true);
        
        //ok
        var res = await function.LeaveHandlerAsync(request, lambdaContext);
        Assert.Equal(200, res.StatusCode);

        //Throw 
        pubSubService.Setup(c => c.RemoveConnectionFromGroupAsync(hub, group, connectionId)).ThrowsAsync(new Exception());

        res = await function.LeaveHandlerAsync(request, lambdaContext);
        
        Assert.Equal(500, res.StatusCode);
        
        //Not authorized
        pubSubService.Setup(c => c.CheckPermissionAsync(hub, PubSubPermission.JoinOrLeaveGroup, connectionId, group))
            .ReturnsAsync(false);
        
        res = await function.LeaveHandlerAsync(request, lambdaContext);
        Assert.Equal(403, res.StatusCode);
        
        //Ack invalid
        pubSubService.Setup(c => c.CreateAckAsync(hub, connectionId, It.IsAny<string?>())).ReturnsAsync(false);
        
        res = await function.LeaveHandlerAsync(request, lambdaContext);
        Assert.Equal(400, res.StatusCode);
    }
    
    [Fact]
    public async Task TestSend()
    {
        var pubSubService = new Mock<IPubSubService>();
        
        var function = new CustomPubSubFunction(pubSubService.Object);
        
        var lambdaContext = new TestLambdaContext();
        
        var connectionId = "test-id";
        var hub = "hub";
        var group = "group";
        
        var body = new PubSubMessage
        {
            type = "send",
            dataType = "json",
            group = group,
            data = new
            {
                h = "h"
            },
            ackId = "$"
        };
        
        var request = new APIGatewayProxyRequest
        {
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                ConnectionId = connectionId,
                Authorizer = new APIGatewayCustomAuthorizerContext
                {
                    ["sub"] = "user",
                    ["nuageshub"] = hub
                }
            },
            Body = JsonSerializer.Serialize(body)
        };

        pubSubService.Setup(c => c.CreateAckAsync(hub, connectionId, It.IsAny<string?>())).ReturnsAsync(true);
        pubSubService.Setup(c => c.CheckPermissionAsync(hub, PubSubPermission.SendMessageToGroup, connectionId, group))
            .ReturnsAsync(true);
        
        pubSubService.Setup(c => c.SendToGroupAsync(hub, group, It.IsAny<PubSubMessage>(), It.IsAny<List<string>?>())).ReturnsAsync(new APIGatewayProxyResponse
        {
            StatusCode = 200
        });

        //Ok 
        var res = await function.SendHandlerAsync(request, lambdaContext);
        Assert.Equal(200, res.StatusCode);

        //Throw exception
        pubSubService.Setup(c => c.CheckPermissionAsync(hub, PubSubPermission.SendMessageToGroup, connectionId, group)).ThrowsAsync(new Exception());
        
        res = await function.SendHandlerAsync(request, lambdaContext);
        Assert.Equal(500, res.StatusCode);
        
        //Not authorized
        pubSubService.Setup(c => c.CheckPermissionAsync(hub, PubSubPermission.SendMessageToGroup, connectionId, group))
            .ReturnsAsync(false);
        
        res = await function.SendHandlerAsync(request, lambdaContext);
        Assert.Equal(403, res.StatusCode);
        
        //Ack not valid
        pubSubService.Setup(c => c.CreateAckAsync(hub, connectionId, It.IsAny<string?>())).ReturnsAsync(false);
        
        res = await function.SendHandlerAsync(request, lambdaContext);
        Assert.Equal(400, res.StatusCode);
    }
    
    [Fact]
    public async Task TestAuthorize()
    {
        var pubSubService = new Mock<IPubSubService>();
        
        var function = new CustomPubSubFunction(pubSubService.Object);
        
        var lambdaContext = new TestLambdaContext();
        
        string connectionId = "test-id";
        
        var request = new APIGatewayCustomAuthorizerRequest
        {
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                ConnectionId = connectionId,
                Authorizer = new APIGatewayCustomAuthorizerContext
                {
                    
                }
            },
            QueryStringParameters = new Dictionary<string, string>()
        };

        var options = function.PubSubOpt.Value;

        request.QueryStringParameters.Add("hub", "Hub");
        request.QueryStringParameters.Add("access_token", GenerateToken(options.Issuer!, options.ValidAudiences!, "userId", new List<string>(), options.Secret! ,null));
        
        var res = await function.OnAuthorizeHandlerAsync(request, lambdaContext);
        
        Assert.NotNull(res.PrincipalID);

        request.QueryStringParameters.Remove("hub");
        
        res = await function.OnAuthorizeHandlerAsync(request, lambdaContext);
        
        Assert.Equal("user", res.PrincipalID);
        
        request.QueryStringParameters.Remove("access_token");
        
        res = await function.OnAuthorizeHandlerAsync(request, lambdaContext);
        
        Assert.Equal("user", res.PrincipalID);
    }
    
    [Fact]
    public async Task TestAuthorizeFailedBadIssuer()
    {
        var pubSubService = new Mock<IPubSubService>();
        
        var function = new CustomPubSubFunction(pubSubService.Object);
        
        var lambdaContext = new TestLambdaContext();
        
        string connectionId = "test-id";
        
        var request = new APIGatewayCustomAuthorizerRequest
        {
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                ConnectionId = connectionId,
                Authorizer = new APIGatewayCustomAuthorizerContext
                {
                    
                }
            },
            QueryStringParameters = new Dictionary<string, string>()
        };

        var options = function.PubSubOpt.Value;

        request.QueryStringParameters.Add("hub", "Hub");
        request.QueryStringParameters.Add("access_token", GenerateToken("bad_issuer", options.ValidAudiences!, "userId", new List<string>(), options.Secret! ,null));
        
        var res = await function.OnAuthorizeHandlerAsync(request, lambdaContext);
        
        Assert.Equal("user", res.PrincipalID);
    }
    
    string GenerateToken(string issuer, string audience, string userId, IEnumerable<string> roles, string secret, TimeSpan? expireDelay = default)
    {
        var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new []
            {
                new Claim("sub", userId),
                new Claim("test", "test"),
                new Claim("test", "test2"),
            }),
            Expires = DateTime.UtcNow.Add(expireDelay ?? TimeSpan.FromDays(1)),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(mySecurityKey, SecurityAlgorithms.HmacSha256Signature)
        };

        var roleList = roles.ToList();
        
        if (roleList.Any())
        {
            tokenDescriptor.Claims ??= new Dictionary<string, object>();

            tokenDescriptor.Claims.Add(new KeyValuePair<string, object>("roles", roleList));
        }

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}