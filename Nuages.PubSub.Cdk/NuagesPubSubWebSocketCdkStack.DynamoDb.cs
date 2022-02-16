using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;

namespace Nuages.PubSub.Cdk;

public partial class NuagesPubSubWebSocketCdkStack<T>
{
    protected virtual void CreateTables()
    {
        // ReSharper disable once UnusedVariable
        var pubSubConnection = new Table(this, "pub_sub_connection", new TableProps
        {
            TableName = TableNamePrefix + "pub_sub_connection",
            BillingMode = BillingMode.PAY_PER_REQUEST,
            PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
            {
                Name = "Id",
                Type = AttributeType.STRING
            },
            RemovalPolicy = RemovalPolicy.DESTROY
        });
        
        // pubSubConnection.AddGlobalSecondaryIndex(new GlobalSecondaryIndexProps
        // {
        //     IndexName = "HubAndConnectionId",
        //     ProjectionType = ProjectionType.ALL,
        //     PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
        //     {
        //         Name = "HubAndConnectionId",
        //         Type = AttributeType.STRING
        //     }
        // });
        //
        // pubSubConnection.AddGlobalSecondaryIndex(new GlobalSecondaryIndexProps
        // {
        //     IndexName = "HubAndUserId",
        //     ProjectionType = ProjectionType.ALL,
        //     PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
        //     {
        //         Name = "HubAndUserId",
        //         Type = AttributeType.STRING
        //     }
        // });


        // ReSharper disable once UnusedVariable
        var pubSubAck = new Table(this, "pub_sub_ack", new TableProps
        {
            TableName = TableNamePrefix + "pub_sub_ack",
            BillingMode = BillingMode.PAY_PER_REQUEST,
            PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
            {
                Name = "Id",
                Type = AttributeType.STRING
            },
            RemovalPolicy = RemovalPolicy.DESTROY
        });
        
        // pubSubAck.AddGlobalSecondaryIndex(new GlobalSecondaryIndexProps
        // {
        //     IndexName = "HubAndConnectionIdAndAckId",
        //     ProjectionType = ProjectionType.ALL,
        //     PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
        //     {
        //         Name = "HubAndConnectionIdAndAckId",
        //         Type = AttributeType.STRING
        //     }
        // });

        // ReSharper disable once UnusedVariable
        var pubSubGroupConnection = new Table(this, "pub_sub_group_connection", new TableProps
        {
            TableName = TableNamePrefix + "pub_sub_group_connection",
            BillingMode = BillingMode.PAY_PER_REQUEST,
            PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
            {
                Name = "Id",
                Type = AttributeType.STRING
            },
            RemovalPolicy = RemovalPolicy.DESTROY
        });
        
        // pubSubGroupConnection.AddGlobalSecondaryIndex(new GlobalSecondaryIndexProps
        // {
        //     IndexName = "HubAndGroup",
        //     ProjectionType = ProjectionType.ALL,
        //     PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
        //     {
        //         Name = "HubAndGroup",
        //         Type = AttributeType.STRING
        //     }
        // });
        //
        // pubSubGroupConnection.AddGlobalSecondaryIndex(new GlobalSecondaryIndexProps
        // {
        //     IndexName = "HubAndGroupAndConnectionId",
        //     ProjectionType = ProjectionType.ALL,
        //     PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
        //     {
        //         Name = "HubAndGroupAndConnectionId",
        //         Type = AttributeType.STRING
        //     }
        // });
        //
        // pubSubGroupConnection.AddGlobalSecondaryIndex(new GlobalSecondaryIndexProps
        // {
        //     IndexName = "HubAndConnectionId",
        //     ProjectionType = ProjectionType.ALL,
        //     PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
        //     {
        //         Name = "HubAndConnectionId",
        //         Type = AttributeType.STRING
        //     }
        // });
        //
        // pubSubGroupConnection.AddGlobalSecondaryIndex(new GlobalSecondaryIndexProps
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
        // pubSubGroupConnection.AddGlobalSecondaryIndex(new GlobalSecondaryIndexProps
        // {
        //     IndexName = "HubAndGroupAndUserId",
        //     ProjectionType = ProjectionType.ALL,
        //     PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
        //     {
        //         Name = "HubAndGroupAndUserId",
        //         Type = AttributeType.STRING
        //     }
        // });

        // ReSharper disable once UnusedVariable
        var pubSubGroupUser = new Table(this, "pub_sub_group_user", new TableProps
        {
            TableName = TableNamePrefix + "pub_sub_group_user",
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