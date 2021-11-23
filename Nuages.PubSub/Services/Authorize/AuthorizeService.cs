using System.IdentityModel.Tokens.Jwt;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Nuages.PubSub.Services.Authorize;

// ReSharper disable once UnusedType.Global
public class AuthorizeService : PubSubServiceBase, IAuthorizeService
{
    private readonly IConfiguration _configuration;

    public AuthorizeService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task<APIGatewayCustomAuthorizerResponse> Authorize(APIGatewayCustomAuthorizerRequest input, ILambdaContext context)
    {
        var architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture;
        var dotnetVersion = Environment.Version.ToString();
        context.Logger.LogLine(
            $"Architecture: {architecture}, .NET Version: {dotnetVersion}");

            
        var token = input.QueryStringParameters["authorization"];
        if (string.IsNullOrEmpty(token))
            return CreateResponse(false, input.MethodArn);

        var claimDict = GetClaims(token);

        var keys = await GetSigningKeys(claimDict["iss"], context);

        context.Logger.LogLine($"iss: {claimDict["iss"]}");
            
        context.Logger.LogLine($"Valid issuers : {_configuration.GetSection("Nuages:Auth:Issuers").Value}");
        context.Logger.LogLine($"Valid audiences : {_configuration.GetSection("Nuages:Auth:Audiences").Value}");
            
        var validIssuers = _configuration.GetSection("Nuages:Auth:Issuers").Value.Split(",");
        var validAudiences = _configuration.GetSection("Nuages:Auth:Audiences").Value.Split(",");

        try
        {
            new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
            {
                IssuerSigningKeys = keys,
                ValidIssuers = validIssuers,
                ValidateIssuer = true,
                ValidAudiences = validAudiences,
                ValidateAudience = true
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
        var endpoint = GetEndpoint(issuer);

        var jsonWebKeySetUrl =  $"{endpoint}.well-known/jwks.json";
            
        context.Logger.LogLine(jsonWebKeySetUrl);
            
        var disableSslCheck = Convert.ToBoolean(_configuration.GetSection("Nuages:DisableSslCheck").Value) ;

        using var handler = new HttpClientHandler();

        if (disableSslCheck)
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