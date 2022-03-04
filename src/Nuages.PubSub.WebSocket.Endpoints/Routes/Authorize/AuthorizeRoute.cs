using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Nuages.AWS.Secrets;
using Nuages.PubSub.Services;

namespace Nuages.PubSub.WebSocket.Endpoints.Routes.Authorize;

// ReSharper disable once UnusedType.Global
// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class AuthorizeRoute : IAuthorizeRoute
{
    private readonly IAWSSecretProvider _secretProvider;
    private readonly PubSubOptions _pubSubOptions;

    public AuthorizeRoute(IOptions<PubSubOptions> pubSubAuthOptions, IAWSSecretProvider secretProvider)
    {
        _secretProvider = secretProvider;
        _pubSubOptions = pubSubAuthOptions.Value;
    }

    public async Task<APIGatewayCustomAuthorizerResponse> AuthorizeAsync(APIGatewayCustomAuthorizerRequest input,
        ILambdaContext context)
    {
        var token = input.QueryStringParameters.ContainsKey("access_token") ? input.QueryStringParameters["access_token"] : null;
        if (string.IsNullOrEmpty(token))
        {
            context.Logger.LogLine("Token (access_token query parameter) was not provided. Exiting.");
            return CreateResponse(false, input.MethodArn);
        }

        var hub = input.QueryStringParameters.ContainsKey("hub") ? input.QueryStringParameters["hub"] : null;

        if (string.IsNullOrEmpty(hub))
        {
            context.Logger.LogLine("Hub (hub query parameter) ws not provided. Exiting.");
            return CreateResponse(false, input.MethodArn);
        }

        context.Logger.LogLine($"Token={token} Hub={hub}");
        
        var jwtToken = new JwtSecurityTokenHandler()
            .ReadJwtToken(token);

        var claimDict = GetClaims(jwtToken);

        claimDict.Add("nuageshub", hub);

        var validIssuer = _pubSubOptions.Auth.Issuer;
        var validAudience = _pubSubOptions.Auth.Audience;

        var keys = await LoadKeys(context);

        try
        {
            ValidateToken(token, keys, validIssuer , validAudience);

            return CreateResponse(true, input.MethodArn, claimDict);
        }
        catch (Exception ex)
        {
            context.Logger.Log(ex.Message);
            return CreateResponse(false, input.MethodArn);
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private class SecretValue
    {
        public string Value { get; set; } = string.Empty;
    }
    
    [ExcludeFromCodeCoverage]
    private async Task<List<SecurityKey>> LoadKeys(ILambdaContext context)
    {
        context.Logger.LogLine($"Input Secret : {_pubSubOptions.Auth.Secret}");
        
        if (string.IsNullOrEmpty(_pubSubOptions.Auth.Secret))
            throw new NullReferenceException("secret was not provided");

        var secret = await _secretProvider.GetSecretAsync<SecretValue>(_pubSubOptions.Auth.Secret);
        
        if (secret == null)
            throw new NullReferenceException("secret can't be read");
        
        context.Logger.LogLine($"Secret : {secret.Value}");
        
        var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret.Value));
        var keys = new List<SecurityKey> { mySecurityKey };

        return await  Task.FromResult(keys);
    }

    [ExcludeFromCodeCoverage]
    protected virtual void ValidateToken(string token, IEnumerable<SecurityKey> keys, string issuer,
        string audience)
    {
        new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = keys,
            ValidIssuer = issuer,
            ValidateIssuer = true,
            ValidAudience = audience
        }, out _);
    }

    public static Dictionary<string, string> GetClaims(JwtSecurityToken token)
    {
        var claimDict = new Dictionary<string, string>();
        foreach (var c in token.Claims)
            if (!claimDict.ContainsKey(c.Type))
                claimDict.Add(c.Type, c.Value);
            else
                claimDict[c.Type] = claimDict[c.Type] + " " + c.Value;
        return claimDict;
    }

    public static APIGatewayCustomAuthorizerResponse CreateResponse(bool success, string methodArn,
        Dictionary<string, string>? claims = null)
    {
        string? principal = null;
        claims?.TryGetValue("sub", out principal);

        var contextOutput = new APIGatewayCustomAuthorizerContextOutput();

        if (claims != null)
            foreach (var keyValuePair in claims.Keys)
                contextOutput[keyValuePair] = claims[keyValuePair];

        return new APIGatewayCustomAuthorizerResponse
        {
            PrincipalID = principal ?? "user",
            PolicyDocument = new APIGatewayCustomAuthorizerPolicy
            {
                Statement =
                {
                    new APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement
                    {
                        Action = new HashSet<string> { "execute-api:Invoke" },
                        Effect = success
                            ? "Allow"
                            : "Deny",
                        Resource = new HashSet<string> { methodArn }
                    }
                }
            },
            Context = contextOutput
        };
    }
}