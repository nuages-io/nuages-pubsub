using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Nuages.PubSub.Services;

namespace Nuages.PubSub.WebSocket.Endpoints.Routes.Authorize;

// ReSharper disable once UnusedType.Global
// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
[ExcludeFromCodeCoverage]
public class AuthorizeRouteExternal : IAuthorizeRoute
{
    private readonly PubSubOptions _pubSubOptions;

    public AuthorizeRouteExternal(IOptions<PubSubOptions> pubSubOptions)
    {
        _pubSubOptions = pubSubOptions.Value;
    }

    public async Task<APIGatewayCustomAuthorizerResponse> AuthorizeAsync(APIGatewayCustomAuthorizerRequest input,
        ILambdaContext context)
    {
        var token = input.QueryStringParameters.ContainsKey("access_token") ? input.QueryStringParameters["access_token"] : null;
        if (string.IsNullOrEmpty(token))
        {
            context.Logger.LogLine("Token (access_token query parameter) was not provided. Exiting.");
            return AuthorizeRoute.CreateResponse(false, input.MethodArn);
        }

        var hub = input.QueryStringParameters.ContainsKey("hub") ? input.QueryStringParameters["hub"] : null;

        if (string.IsNullOrEmpty(hub))
        {
            context.Logger.LogLine("Hub (hub query parameter) ws not provided. Exiting.");
            return AuthorizeRoute.CreateResponse(false, input.MethodArn);
        }

        var jwtToken = new JwtSecurityTokenHandler()
            .ReadJwtToken(token);

        var claimDict = AuthorizeRoute.GetClaims(jwtToken);

        claimDict.Add("nuageshub", hub);

        context.Logger.LogInformation("PubSubOptions=" + JsonSerializer.Serialize(_pubSubOptions));
        
        var validIssuers = _pubSubOptions.ExternalAuth.ValidIssuers.Split(",");
        var validAudiences = _pubSubOptions.ExternalAuth.ValidAudiences?.Split(",").ToList();

        var keys = await LoadKeys(context, jwtToken);

        try
        {
            ValidateToken(token, keys, validIssuers, validAudiences);

            return AuthorizeRoute.CreateResponse(true, input.MethodArn, claimDict);
        }
        catch (Exception ex)
        {
            context.Logger.Log(ex.Message);
            return AuthorizeRoute.CreateResponse(false, input.MethodArn);
        }
    }

    [ExcludeFromCodeCoverage]
    private async Task<List<SecurityKey>> LoadKeys(ILambdaContext context, SecurityToken jwtToken)
    {
        var keys = new List<SecurityKey>();

        keys.AddRange(await GetSigningKeys(jwtToken.Issuer, context));

        return keys;
    }

    [ExcludeFromCodeCoverage]
    protected virtual void ValidateToken(string token, IEnumerable<SecurityKey> keys, IEnumerable<string> validIssuers,
        IEnumerable<string>? validAudiences)
    {
        
        new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = keys,
            ValidIssuers = validIssuers,
            ValidateIssuer = true,
            ValidAudiences = validAudiences,
            ValidateAudience = true
        }, out _);
    }

    [ExcludeFromCodeCoverage]
    private static string GetEndpoint(string issuer)
    {
        var endpoint = issuer;

        if (!endpoint.EndsWith("/"))
            endpoint += "/";

        return endpoint;
    }

    [ExcludeFromCodeCoverage]
    protected virtual async Task<IList<JsonWebKey>> GetSigningKeys(string issuer, ILambdaContext context)
    {
        var jsonWebKeySetUrl = $"{GetEndpoint(issuer)}{_pubSubOptions.ExternalAuth.JsonWebKeySetUrlPath}";

        context.Logger.LogLine(jsonWebKeySetUrl);

        using var handler = new HttpClientHandler();

        if (_pubSubOptions.ExternalAuth.DisableSslCheck)
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;

        using var httpClient = new HttpClient(handler);

        var response = await httpClient.GetAsync(jsonWebKeySetUrl);
        var issuerJsonWebKeySet = new JsonWebKeySet(await response.Content.ReadAsStringAsync());

        var keys = issuerJsonWebKeySet.Keys;
        return keys;
    }
}