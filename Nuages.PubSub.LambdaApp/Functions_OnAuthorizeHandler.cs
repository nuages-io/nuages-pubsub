#region

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.IdentityModel.Tokens;

#endregion

namespace Nuages.PubSub.LambdaApp
{
    // ReSharper disable once UnusedType.Global
    public partial class Functions
    {
        // ReSharper disable once UnusedMember.Global
        public async Task<APIGatewayCustomAuthorizerResponse> OnAuthorizeHandlerAsync(
            APIGatewayCustomAuthorizerRequest input,
            ILambdaContext context)
        {
            //context.Logger.Log(JsonSerializer.Serialize(input));

            //var issuer = Environment.GetEnvironmentVariable("ISSUER");
            var disableSslCheck = false;

            var audience = input.QueryStringParameters["client_id"];
            if (string.IsNullOrEmpty(audience))
                return CreateResponse(false, input.MethodArn);

            var token = input.QueryStringParameters["authorization"];
            if (string.IsNullOrEmpty(token))
                return CreateResponse(false, input.MethodArn);

            var claims = new JwtSecurityTokenHandler()
                .ReadJwtToken(token)
                .Claims;

            var claimDict = new Dictionary<string, string>();
            foreach (var c in claims)
                if (!claimDict.ContainsKey(c.Type))
                    claimDict.Add(c.Type, c.Value);
                else
                    claimDict[c.Type] = claimDict[c.Type] + " " + c.Value;

            var issuer = claimDict["iss"];
            var endpoint = issuer;
            
            if (endpoint == "https://localhost:8001")
            {
                endpoint = "https://auth-nuages.ngrok.io";
            }

            if (endpoint.ToLower().Contains("ngrok.io"))
            {
                disableSslCheck = true;
            }
            
            var jsonWebKeySetUrl = endpoint.EndsWith("/")
                ? endpoint + ".well-known/openid-configuration/jwks"
                : endpoint + "/.well-known/openid-configuration/jwks";

            using var handler = new HttpClientHandler();

            if (disableSslCheck) handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;

            using var httpClient = new HttpClient(handler);

            var response = await httpClient.GetAsync(jsonWebKeySetUrl);
            var issuerJsonWebKeySet = new JsonWebKeySet(await response.Content.ReadAsStringAsync());

            try
            {
                new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
                {
                    IssuerSigningKeys = issuerJsonWebKeySet.Keys,
                    ValidIssuer = issuer,
                    ValidateIssuer = true,
                    ValidAudience = "API",
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
    }
}