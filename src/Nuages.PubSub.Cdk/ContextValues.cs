namespace Nuages.PubSub.Cdk;

// ReSharper disable once InconsistentNaming
public static class ContextValues
{
    public const string WebSocketDomainName = "WebSocket_Domain";
    public const string WebSocketCertificateArn = "WebSocket_CertificateArn";

    public const string ApiDomainName = "API_Domain";
    public const string ApiCertificateArn = "API_CertificateArn";
    public const string ApiApiKey = "API_ApiKey";
    
    public const string AuthAudience = "Auth_Audience";
    public const string AuthIssuer = "Auth_Issuer";
    public const string AuthSecret = "Auth_Secret";

    public const string VpcId = "Vpc_Id";

    public const string DatabaseProxyArn = "DatabaseProxy_Arn";
    public const string DatabaseProxyName = "DatabaseProxy_Name";
    public const string DatabaseProxyEndpoint = "DatabaseProxy_Endpoint";
    public const string DatabaseProxyUser = "DatabaseProxy_User";
    public const string DatabaseProxySecurityGroup = "DatabaseProxy_SecurityGroup_Id";
    
    public const string DataStorage = "Data_Storage";
    public const string DataPort = "Data_Port";
    public const string DataConnectionString = "Data_ConnectionString";
    public const string DataCreateDynamoDbTables = "Data_CreateDynamoDbTables";

}