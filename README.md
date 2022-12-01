# Nuages PubSub

Nuages.PubSub is a WebSocket services based on API Gateway WebSocket. It provides all the building block to easily deploy the backend to Lambda using Cloud Formation.

### At a glance:

- Similar to Azure Web PubSub
- Include WebSocket Service + Client API
- Open API endpoint available (/swagger)
- Many database options are available: DynamoDB, MongoDB, MySQL abnd SQL Server. More can be added by providing additional provider trough DI.
- Use the following AWS services
  - API Gateway
  - Lambda
  - CloudFormation (CDK)
  - IAM
  - CloudWatch
  - Route 53 for custom domain (optional)
  - Certificate Manager (optional)
  - System Manager (Parameter Store, App Config, optional)
  - DynamoDB (optional)



## Prerequisites

**IMPORTANT!** You need to have CDK configured on your machine
**IMPORTANT!** You need .NET 6.

https://docs.aws.amazon.com/cdk/v2/guide/work-with.html#work-with-prerequisites


## Getting Started

Everything is ready to be used as-is. All you need to do is deploy using the CDK from you machine.

1. git clone https://github.com/nuages-io/nuages-pubsub
3. cdk deploy
4. Wait for the deployment to complete

WebSocket Endpoint Url and API Endpoint Url will be outputed in the console window upon deployment completion. The API Key required to call the API is available via the AWS console in the API Gateway section if you did not provide a value, 

That's it. Your Web Socket services and API are ready to go.

## Customizing the deployment stack

You can customize most of the settings to fit you needs. Just follow the instructions below.


#### 1. Clone this Github repository on your machine

```
git clone https://github.com/nuages-io/nuages-pubsub
```
#### 2. Setup CDK deployment options

The following context variable can be set to customize the deployment using the CDK. (See https://docs.aws.amazon.com/cdk/v2/guide/context.html for detail about context).

By default, the stack will be deployed with the current configuration

- Not part of a Vpc
- Auto generated URL for WebSocket Endpoint and API Endpoint
- DynamoDB as internal storage
- Auto-generated API key
- Default values for Issuer, Audience and Secret.

You can customize this behavior by setting the following options.



#### Infrastructure options

| Name                                   | Description                        | Instructions                                                 |
| -------------------------------------- | ---------------------------------- | ------------------------------------------------------------ |
| ConfigOptions__WebSocketDomain         | WebSocket Endpoint Domain Name     | DO NOT CREATE YOURSELF. Route53 recordset will be created with the stack |
| ConfigOptions__WebSocketCertificateArn | Certificate for WebSocket Endpoint | Required if WebSocketDomain is assigned.                     |
| ConfigOptions__ApiDomain               | API Endpoint Domain Name           | DO NOT CREATE YOURSELF. Route53 recordset will be created with the stack |
| ConfigOptions__ApiCertificateArn       | Certificate for API Endpoint       | Required if APIDomain is assigned                            |
| ConfigOptions__ApiApiKey               | API Gateway API Key                | Will be autogenerated if not provided                        |
| ConfigOptions__VpcId                   | Id of the VPC.                     | Must be the VPC of the database you connect to. If you connect to an Internet data source like MongoDb Atlas you will have to make sure the Lambda has internet access. |
| ConfigOptions__SecurityGroupId         | Security group Id                  | The Lambdas will be added to this security group. If a database proxy is used, this security group must have access to the database proxy and the database |

#### Database Proxy Options

The following options applies if you choose to use a DatabaseProxy for your MySQL database. Both the VPC and DatabaseProxy must exists and will NOT be created during deployment. All options are required.

**IMPORTANT!** Adding the lambda to a VPC will prevent outbound access to the Internet if the VPC does not provided a public subnet and an Internet Gateway. Nuages PubSub does not require a Internet connection, so this might not be an issue unless you customize the code and add services that connect to the Internet.

| Name                       | Description                | Instructions                                                 |
| -------------------------- | -------------------------- | ------------------------------------------------------------ |
|                       |               |                 |
| ConfigOptions\__DatabaseProxy__User | Database user name         | Must be the same as in the connection string                 |
| ConfigOptions\__DatabaseProxy__Arn | ARN of the Database Proxy | Required. See Proxy properties. |
| ConfigOptions\__DatabaseProxy__Endpoint | Proxy Endpoint            | Required. See Proxy properties. |
| ConfigOptions\__DatabaseProxy__Name | Name of the proxy         | Required. See Proxy properties. |



#### Database options

| Name                                    | Description                                  | Instructions                                                 |
| --------------------------------------- | -------------------------------------------- | ------------------------------------------------------------ |
| RuntimeOptions\__Data__Storage          | Database storage used. Default: DynamoDB     | DynamoDB, MySQL, SQLServer or MongoDB                        |
| RuntimeOptions\__Data__ConnectionString | Connection string to connect to the database | May be a connection string, an AWS Secrets Arn or null if provided from application settings. |

**IMPORTANT!!!**

- Only DynamoDB tables will be deployed as part of the stack. For other database engines, you need to do this deploy on your own and provide the connection string.
- It is recommended to setup a DatabaseProxy for MySQL and SQL Server (https://docs.aws.amazon.com/lambda/latest/dg/configuration-database.html)



#### Authentication settings

The following settings can be set at deployment time. You may also want to set the values using standard application settings (see following section).

| Name                           | Description    | Instructions                                             |
| ------------------------------ | -------------- | -------------------------------------------------------- |
|                                |                |                                                          |
| RuntimeOptions\__Auth_Issuer   | Token issuer   | Ex: MyCompany or https://mycompany.com                   |
| RuntimeOptions\__Auth_Audience | Token audience | Ex: MyAudience                                           |
| RuntimeOptions\__Auth_Secret   | Token secret   | Ex. Oiwebwbehj439857948fekhekrht435 (any value you want) |

The following settings applies when you want to use External Authentication

| Name                                               | Description                                  | Instructions                                                 |
| -------------------------------------------------- | -------------------------------------------- | ------------------------------------------------------------ |
|                                                    |                                              |                                                              |
| RuntimeOptions\__ExternalAuth_Enabled              | Enabled when True                            |                                                              |
| RuntimeOptions\__ExternalAuth_ValidAudiences       | List of valid audiences                      | Comma separated                                              |
| RuntimeOptions\__ExternalAuth_ValidIssuers         | List of valid isseurs URL                    | Must be URLs. Comma separated                                |
| RuntimeOptions\__ExternalAuth_DisableSslCheck      | Disable SSL certificate validation when True | Might be useful if you are using ngrok                       |
| RuntimeOptions\__ExternalAuth_JsonWebKeySetUrlPath | Json Web Token Key Set Path.                 | Open OpenId configuration page for your issuer. Ex: https://myissuer.com/.well-known/openid-configuration . Look for the wks_url value. Enter only the Path element. |
| RuntimeOptions\__ExternalAuth_Roles                | Roles asisgned to connections                | Default to "SendMessageToGroup JoinOrLeaveGroup"             |



### 4. Deploy

Run this command at the solution root

```
cdk deploy
```



# How to use Nuages PubSub

Very quick intructions here...I will be working on something more complete soon.

- Services URL are available in the Output tab of the Cloud Formation Nuages PubSub stack page

  - One endpoint for WebSocket
  - One endpoint for the API


- You can get the WebSocket URL using the **API Endpoint** getclienturi method


The API endpoint is secured usingh the API Key you provided on deploy (or it has been automatically assign). You can retrieve the value from the API Keys section in the AWS Application Gateway page.

The API key value should be added to the **x-api-key** request header

#### Get a Client Url

```
curl --location --request GET \
'[Your-API-Url]/api/auth/getclienturi?userId=martin&roles=SendMessageToGroup&roles=JoinOrLeaveGroup' \
--header 'x-api-key: [Your-Api-Key]'
```

This will get a WebSocket client Url

#### Connect

Once you have the WebSocket Url, you can use it to connect using any WebSocket client

https://docs.aws.amazon.com/apigateway/latest/developerguide/apigateway-how-to-call-websocket-api-wscat.html

```
wscat -c "[Your-WebSocket-Client-Url]"
```

#### Echo

```json
{ "type":"echo" }
```

#### Join

```json
{ "type":"join","dataType" : "json","group" : "group1"}
```

#### Leave

```json
{ "type":"leave", "dataType" : "json", "group" : "group1"}
```

#### SendMessage

```json
{"type":"send", "dataType" : "json", "group" : "group1", "data": { "my_message" : "message sent to group" }}
```

Additional capabilities are offered using the API Endpoint.

## API / SDK

API Documentaton is available here https://app.swaggerhub.com/apis-docs/Nuages/NuagesPubSub/1.0.0#/

C# SDK is available here https://www.nuget.org/packages/Nuages.PubSub.API.Sdk/
