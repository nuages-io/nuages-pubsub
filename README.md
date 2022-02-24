# Nuages PubSub

Nuages.PubSub is a WebSocket services based on API Gateway WebSocket. It provides all the building block to easily deploy the backend to Lambda using Cloud Formation.

### At a glance:

- Similar to Azure Web PubSub
- Include WebSocket Service + Client API
- .NET Core 6 SDK included
- Open API endpoint available (/swagger)
- Many database options are available: DynamoDb, MongoDb and MySql. More can be added by providing additional provider trough DI.
- Use the following AWS services
  - API Gateway
  - Lambda
  - CloudFront (CDK)
  - IAM
  - CloudWatch
  - Route 53 for custom domain (optional)
  - Certificate Manager (optional)
  - System Manager (parameter store, optional)
  - DynamoDb (optional)

## Getting Started

See https://github.com/nuages-io/nuages-pubsub-starter for deployment options.