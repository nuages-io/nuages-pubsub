using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Nuages.PubSub.Services;
using Nuages.PubSub.WebSocket.Routes.JoinLeave;
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
        request.RequestContext.Authorizer["roles"] = null;
        
        await function.OnConnectHandlerAsync(request, lambdaContext);

        pubSubService.Setup(c => c.ConnectAsync(hub, connectionId, sub, It.IsAny<TimeSpan?>())).ThrowsAsync(new Exception());

        var res = await function.OnConnectHandlerAsync(request, lambdaContext);
        
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
        
        await function.OnDisconnectHandlerAsync(request, lambdaContext);
        
        pubSubService.Setup(c => c.CloseConnectionAsync(hub, connectionId)).ThrowsAsync(new Exception());

        var res = await function.OnDisconnectHandlerAsync(request, lambdaContext);
        
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

        await function.EchoHandlerAsync(request, lambdaContext);
        
        pubSubService.Setup(c => c.SendToConnectionAsync(It.IsAny<string>(), connectionId, It.IsAny<PubSubMessage>())).ThrowsAsync(new Exception());

        var res = await function.EchoHandlerAsync(request, lambdaContext);
        
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
            }
        };

        await function.JoinHandlerAsync(request, lambdaContext);
        
        pubSubService.Setup(c => c.AddUserToGroupAsync(hub, group, user)).ThrowsAsync(new Exception());

        var res = await function.JoinHandlerAsync(request, lambdaContext);
        
        Assert.Equal(500, res.StatusCode);
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
            }
        };

        await function.LeaveHandlerAsync(request, lambdaContext);
        
        pubSubService.Setup(c => c.RemoveUserFromGroupAsync(hub, group, user)).ThrowsAsync(new Exception());

        var res = await function.LeaveHandlerAsync(request, lambdaContext);
        
        Assert.Equal(500, res.StatusCode);
    }
    
    [Fact]
    public async Task TestSend()
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

        await function.SendHandlerAsync(request, lambdaContext);
        
        // pubSubService.Setup(c => c.Sen(hub, group, user)).ThrowsAsync(new Exception());
        //
        // var res = await function.SendHandlerAsync(request, lambdaContext);
        //
        // Assert.Equal(500, res.StatusCode);
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
        request.QueryStringParameters.Add("access_token", GenerateToken(options.Issuer!, options.ValidAudiences!, "user", new List<string>(), options.Secret! ,null));
        
        await function.OnAuthorizeHandlerAsync(request, lambdaContext);
    }
    
    string GenerateToken(string issuer, string audience, string userId, IEnumerable<string> roles, string secret, TimeSpan? expireDelay = default)
    {
        var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new []
            {
                new Claim("sub", userId)
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