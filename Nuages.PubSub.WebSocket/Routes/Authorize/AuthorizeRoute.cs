using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Nuages.PubSub.WebSocket.Model;

namespace Nuages.PubSub.WebSocket.Routes.Authorize;

// ReSharper disable once UnusedType.Global
public class AuthorizeRoute : IAuthorizeRoute
{
    private readonly IConfiguration _configuration;
    private readonly PubSubAuthOptions _pubSubAuthOptions;

    public AuthorizeRoute(IConfiguration configuration, IOptions<PubSubAuthOptions> pubSubAuthOptions)
    {
        _configuration = configuration;
        _pubSubAuthOptions = pubSubAuthOptions.Value;
    }
    
    public async Task<APIGatewayCustomAuthorizerResponse> AuthorizeAsync(APIGatewayCustomAuthorizerRequest input, ILambdaContext context)
    {
        
        var architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture;
        var dotnetVersion = Environment.Version.ToString();
        context.Logger.LogLine(
            $"Architecture: {architecture}, .NET Version: {dotnetVersion}");
            
        var token = input.QueryStringParameters["access_token"];
        if (string.IsNullOrEmpty(token))
            return CreateResponse(false, input.MethodArn);

        var claimDict = GetClaims(token);

        context.Logger.LogLine($"iss: {claimDict["iss"]}");
            
        context.Logger.LogLine($"Valid issuers : {_pubSubAuthOptions.Issuers}");
        context.Logger.LogLine($"Valid audiences : {_pubSubAuthOptions.Audiences}");
            
        var validIssuers = _pubSubAuthOptions.Issuers.Split(",");
        var validAudiences = _pubSubAuthOptions.Audiences?.Split(",");

        List<SecurityKey> keys = new List<SecurityKey>();
        
        var secret = _pubSubAuthOptions.Secret;
        if (!string.IsNullOrEmpty(secret))
        {
            var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
            keys = new List<SecurityKey> { mySecurityKey };
            
            context.Logger.LogLine($"Secret : {secret}");

        }
        else
        {
            keys.AddRange(await GetSigningKeys(claimDict["iss"], context));
        }

        try
        {
            new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = keys,
                ValidIssuers = validIssuers,
                ValidateIssuer = true,
                ValidAudiences = validAudiences,
                ValidateAudience = validAudiences?.Any() ?? false
            }, out _);

            return CreateResponse(true, input.MethodArn, claimDict);
        }
        catch (Exception ex)
        {
            context.Logger.Log(ex.Message);
        }

        return CreateResponse(false, input.MethodArn);
    }
    
    private static string GetEndpoint(string issuer)
    {
        var endpoint = issuer;

        if (!endpoint.EndsWith("/"))
            endpoint += "/";
            
        return endpoint;
    }

    private async Task<IList<JsonWebKey>> GetSigningKeys(string issuer, ILambdaContext context)
    {
        var jsonWebKeySetUrl =  $"{GetEndpoint(issuer)}{_pubSubAuthOptions.JsonWebKeySetUrlPath}";
            
        context.Logger.LogLine(jsonWebKeySetUrl);
            
        using var handler = new HttpClientHandler();

        if (_pubSubAuthOptions.DisableSslCheck)
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;

        using var httpClient = new HttpClient(handler);

        var response = await httpClient.GetAsync(jsonWebKeySetUrl);
        var issuerJsonWebKeySet = new JsonWebKeySet(await response.Content.ReadAsStringAsync());

        var keys = issuerJsonWebKeySet.Keys;
        return keys;
    }

    private static Dictionary<string, string> GetClaims(string token)
    {
        var claims = new JwtSecurityTokenHandler()
            .ReadJwtToken(token)
            .Claims;

        var claimDict = new Dictionary<string, string>();
        foreach (var c in claims)
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
                        Action = new HashSet<string> {"execute-api:Invoke"},
                        Effect = success
                            ? "Allow"
                            : "Deny",
                        Resource = new HashSet<string> {methodArn}
                    }
                }
            },
            Context = contextOutput
        };
    }
}