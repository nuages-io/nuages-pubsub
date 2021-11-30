using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Nuages.PubSub.Services;

namespace Nuages.PubSub.WebSocket.Routes.Authorize;

// ReSharper disable once UnusedType.Global
public class AuthorizeRoute : IAuthorizeRoute
{
    private readonly PubSubOptions _pubSubOptions;

    public AuthorizeRoute(IOptions<PubSubOptions> pubSubAuthOptions)
    {
        _pubSubOptions = pubSubAuthOptions.Value;
    }

    public async Task<APIGatewayCustomAuthorizerResponse> AuthorizeAsync(APIGatewayCustomAuthorizerRequest input,
        ILambdaContext context)
    {
        var token = input.QueryStringParameters["access_token"];
        if (string.IsNullOrEmpty(token))
        {
            context.Logger.LogLine("Token (access_token query parameter) was not provided. Exiting.");
            return CreateResponse(false, input.MethodArn);
        }

        var hub = input.QueryStringParameters["hub"];

        if (string.IsNullOrEmpty(hub))
        {
            context.Logger.LogLine("Hub (hub query parameter) ws not provided. Exiting.");
            return CreateResponse(false, input.MethodArn);
        }

        var jwtToken = new JwtSecurityTokenHandler()
            .ReadJwtToken(token);

        var claimDict = GetClaims(jwtToken);

        claimDict.Add("nuageshub", hub);

        var validIssuers = _pubSubOptions.ValidIssuers.Split(",");
        var validAudiences = _pubSubOptions.ValidAudiences?.Split(",").ToList();

        var keys = new List<SecurityKey>();

        var secret = _pubSubOptions.Secret;
        switch (jwtToken.SignatureAlgorithm)
        {
            case "RS256":
            {
                keys.AddRange(await GetSigningKeys(jwtToken.Issuer, context));
                break;
            }
            default:
            {
                if (string.IsNullOrEmpty(secret))
                    throw new NullReferenceException("secret was not provided");

                var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
                keys = new List<SecurityKey> { mySecurityKey };

                context.Logger.LogLine($"Secret : {secret}");
                break;
            }
        }

        try
        {
            ValidateToken(token, keys, validIssuers, validAudiences);

            return CreateResponse(true, input.MethodArn, claimDict);
        }
        catch (Exception ex)
        {
            context.Logger.Log(ex.Message);
        }

        return CreateResponse(false, input.MethodArn);
    }

    protected virtual void ValidateToken(string token, List<SecurityKey> keys, string[] validIssuers,
        List<string>? validAudiences)
    {
        new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = keys,
            ValidIssuers = validIssuers,
            ValidateIssuer = true,
            ValidAudiences = validAudiences,
            ValidateAudience = validAudiences != null && validAudiences.Any()
        }, out _);
    }

    private static string GetEndpoint(string issuer)
    {
        var endpoint = issuer;

        if (!endpoint.EndsWith("/"))
            endpoint += "/";

        return endpoint;
    }

    protected virtual async Task<IList<JsonWebKey>> GetSigningKeys(string issuer, ILambdaContext context)
    {
        var jsonWebKeySetUrl = $"{GetEndpoint(issuer)}{_pubSubOptions.JsonWebKeySetUrlPath}";

        context.Logger.LogLine(jsonWebKeySetUrl);

        using var handler = new HttpClientHandler();

        if (_pubSubOptions.DisableSslCheck)
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;

        using var httpClient = new HttpClient(handler);

        var response = await httpClient.GetAsync(jsonWebKeySetUrl);
        var issuerJsonWebKeySet = new JsonWebKeySet(await response.Content.ReadAsStringAsync());

        var keys = issuerJsonWebKeySet.Keys;
        return keys;
    }

    protected virtual Dictionary<string, string> GetClaims(JwtSecurityToken token)
    {
        var claimDict = new Dictionary<string, string>();
        foreach (var c in token.Claims)
            if (!claimDict.ContainsKey(c.Type))
                claimDict.Add(c.Type, c.Value);
            else
                claimDict[c.Type] = claimDict[c.Type] + " " + c.Value;
        return claimDict;
    }

    private static APIGatewayCustomAuthorizerResponse CreateResponse(bool success, string methodArn,
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