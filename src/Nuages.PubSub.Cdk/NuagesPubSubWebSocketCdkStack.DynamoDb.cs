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
                Name = "Hub",
                Type = AttributeType.STRING
            },
            RemovalPolicy = RemovalPolicy.DESTROY,
            SortKey = new Amazon.CDK.AWS.DynamoDB.Attribute
            {
                Name = "ConnectionId",
                Type = AttributeType.STRING
            } 
        });
        
        pubSubConnection.AddLocalSecondaryIndex(new LocalSecondaryIndexProps
        {
            SortKey = new Amazon.CDK.AWS.DynamoDB.Attribute
            {
                Name = "UserId",
                Type = AttributeType.STRING
            },
            IndexName = "Connection_UserId",
            ProjectionType = ProjectionType.ALL
        });

        // ReSharper disable once UnusedVariable
        var pubSubAck = new Table(this, "pub_sub_ack", new TableProps
        {
            TableName = StackName + "_pub_sub_ack",
            BillingMode = BillingMode.PAY_PER_REQUEST,
            PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
            {
                Name = "Hub",
                Type = AttributeType.STRING
            },
            RemovalPolicy = RemovalPolicy.DESTROY,
            SortKey = new Amazon.CDK.AWS.DynamoDB.Attribute
            {
                Name = "ConnectionIdAndAckId",
                Type = AttributeType.STRING
            }
        });
        
     

        // ReSharper disable once UnusedVariable
        var pubSubGroupConnection = new Table(this, "pub_sub_group_connection", new TableProps
        {
            TableName = StackName + "_pub_sub_group_connection",
            BillingMode = BillingMode.PAY_PER_REQUEST,
            PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
            {
                Name = "Hub",
                Type = AttributeType.STRING
            },
            RemovalPolicy = RemovalPolicy.DESTROY,
            SortKey = new Amazon.CDK.AWS.DynamoDB.Attribute
            {
                Name = "GroupAndConnectionId",
                Type = AttributeType.STRING
            }
        });
        
        pubSubGroupConnection.AddLocalSecondaryIndex(new LocalSecondaryIndexProps
        {
            SortKey = new Amazon.CDK.AWS.DynamoDB.Attribute
            {
                Name = "ConnectionId",
                Type = AttributeType.STRING
            },
            IndexName = "GroupConnection_ConnectionId",
            ProjectionType = ProjectionType.ALL
        });
        
        pubSubGroupConnection.AddLocalSecondaryIndex(new LocalSecondaryIndexProps
        {
            SortKey = new Amazon.CDK.AWS.DynamoDB.Attribute
            {
                Name = "GroupAndUserId",
                Type = AttributeType.STRING
            },
            IndexName = "GroupConnection_GroupAndUserId",
            ProjectionType = ProjectionType.ALL
        });
        
        pubSubGroupConnection.AddLocalSecondaryIndex(new LocalSecondaryIndexProps
        {
            SortKey = new Amazon.CDK.AWS.DynamoDB.Attribute
            {
                Name = "UserId",
                Type = AttributeType.STRING
            },
            IndexName = "GroupConnection_UserId",
            ProjectionType = ProjectionType.ALL
        });
        
        
        // ReSharper disable once UnusedVariable
        var pubSubGroupUser = new Table(this, "pub_sub_group_user", new TableProps
        {
            TableName = StackName + "_pub_sub_group_user",
            BillingMode = BillingMode.PAY_PER_REQUEST,
            PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
            {
                Name = "Hub",
                Type = AttributeType.STRING
            },
            RemovalPolicy = RemovalPolicy.DESTROY,
            SortKey = new Amazon.CDK.AWS.DynamoDB.Attribute
            {
                Name = "GroupAndUserId",
                Type = AttributeType.STRING
            }
        });
        
        pubSubGroupUser.AddLocalSecondaryIndex(new LocalSecondaryIndexProps
        {
            SortKey = new Amazon.CDK.AWS.DynamoDB.Attribute
            {
                Name = "UserId",
                Type = AttributeType.STRING
            },
            IndexName = "GroupUser_UserId",
            ProjectionType = ProjectionType.ALL
        });
    }
}