using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;

namespace Nuages.PubSub.Cdk;

public partial class NuagesPubSubWebSocketCdkStack<T>
{
    // ReSharper disable once VirtualMemberNeverOverridden.Global
    protected virtual void CreateTables()
    {
        // ReSharper disable once UnusedVariable
        var pubSubConnection = new Table(this, "pub_sub_connection", new TableProps
        {
            TableName = StackName + "_pub_sub_connection",
            BillingMode = BillingMode.PAY_PER_REQUEST,
            PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
            {
                Name = "Id",
                Type = AttributeType.STRING
            },
            RemovalPolicy = RemovalPolicy.DESTROY,
            
        });
        

        // ReSharper disable once UnusedVariable
        var pubSubAck = new Table(this, "pub_sub_ack", new TableProps
        {
            TableName = StackName + "_pub_sub_ack",
            BillingMode = BillingMode.PAY_PER_REQUEST,
            PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
            {
                Name = "Id",
                Type = AttributeType.STRING
            },
            RemovalPolicy = RemovalPolicy.DESTROY
        });
        
     

        // ReSharper disable once UnusedVariable
        var pubSubGroupConnection = new Table(this, "pub_sub_group_connection", new TableProps
        {
            TableName = StackName + "_pub_sub_group_connection",
            BillingMode = BillingMode.PAY_PER_REQUEST,
            PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
            {
                Name = "Id",
                Type = AttributeType.STRING
            },
            RemovalPolicy = RemovalPolicy.DESTROY
        });
        
        // ReSharper disable once UnusedVariable
        var pubSubGroupUser = new Table(this, "pub_sub_group_user", new TableProps
        {
            TableName = StackName + "_pub_sub_group_user",
            BillingMode = BillingMode.PAY_PER_REQUEST,
            PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
            {
                Name = "Id",
                Type = AttributeType.STRING
            },
            RemovalPolicy = RemovalPolicy.DESTROY
        });
        
        // pubSubGroupUser.AddGlobalSecondaryIndex(new GlobalSecondaryIndexProps
        // {
        //     IndexName = "HubAndUserId",
        //     ProjectionType = ProjectionType.ALL,
        //     PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
        //     {
        //         Name = "HubAndUserId",
        //         Type = AttributeType.STRING
        //     }
        // });
        //
        // pubSubGroupUser.AddGlobalSecondaryIndex(new GlobalSecondaryIndexProps
        // {
        //     IndexName = "HubAndGroupAndUserId",
        //     ProjectionType = ProjectionType.ALL,
        //     PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
        //     {
        //         Name = "HubAndGroupAndUserId",
        //         Type = AttributeType.STRING
        //     }
        // });
    }
}