using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;

// ReSharper disable VirtualMemberNeverOverridden.Global

namespace Nuages.PubSub.Deploy.Cdk;

public partial class PubSubWebSocketCdkStack
{
    // ReSharper disable once VirtualMemberNeverOverridden.Global
    private void CreateTables()
    {
        Console.WriteLine("CreateTables");
        
        var pubSubConnection = CreateConnectionTable();
        
        pubSubConnection.AddLocalSecondaryIndex(Get_Connection_UserId_LSIndexProps());

        CreateAckTable();
        
        var pubSubGroupConnection = CreateGroupTable();
        
        pubSubGroupConnection.AddLocalSecondaryIndex(Get_Group_ConnectionId_LSIndexProps());
        pubSubGroupConnection.AddLocalSecondaryIndex(Get_Group_GroupAndUserId_LSIndexProps());
        
        pubSubGroupConnection.AddLocalSecondaryIndex(Get_Group_UserId_LSIndexProps());
        
        var pubSubGroupUser = CreateGroupUserTable();
        
        pubSubGroupUser.AddLocalSecondaryIndex(Get_GroupUser_UserId_LSIndexProps());
    }

    private static LocalSecondaryIndexProps Get_GroupUser_UserId_LSIndexProps()
    {
        return new LocalSecondaryIndexProps
        {
            SortKey = new Amazon.CDK.AWS.DynamoDB.Attribute
            {
                Name = "UserId",
                Type = AttributeType.STRING
            },
            IndexName = "GroupUser_UserId",
            ProjectionType = ProjectionType.ALL
        };
    }

    private Table CreateGroupUserTable()
    {
        return new Table(this, "pub_sub_group_user", new TableProps
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
    }

    private static LocalSecondaryIndexProps Get_Group_UserId_LSIndexProps()
    {
        return new LocalSecondaryIndexProps
        {
            SortKey = new Amazon.CDK.AWS.DynamoDB.Attribute
            {
                Name = "UserId",
                Type = AttributeType.STRING
            },
            IndexName = "GroupConnection_UserId",
            ProjectionType = ProjectionType.ALL
        };
    }

    private static LocalSecondaryIndexProps Get_Group_GroupAndUserId_LSIndexProps()
    {
        return new LocalSecondaryIndexProps
        {
            SortKey = new Amazon.CDK.AWS.DynamoDB.Attribute
            {
                Name = "GroupAndUserId",
                Type = AttributeType.STRING
            },
            IndexName = "GroupConnection_GroupAndUserId",
            ProjectionType = ProjectionType.ALL
        };
    }

    private static LocalSecondaryIndexProps Get_Group_ConnectionId_LSIndexProps()
    {
        return new LocalSecondaryIndexProps
        {
            SortKey = new Amazon.CDK.AWS.DynamoDB.Attribute
            {
                Name = "ConnectionId",
                Type = AttributeType.STRING
            },
            IndexName = "GroupConnection_ConnectionId",
            ProjectionType = ProjectionType.ALL
        };
    }

    private Table CreateGroupTable()
    {
        return new Table(this, "pub_sub_group_connection", new TableProps
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
    }

    // ReSharper disable once UnusedMethodReturnValue.Global
    // ReSharper disable once UnusedMethodReturnValue.Local
    private Table CreateAckTable()
    {
        return new Table(this, "pub_sub_ack", new TableProps
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
    }

    private static LocalSecondaryIndexProps Get_Connection_UserId_LSIndexProps()
    {
        return new LocalSecondaryIndexProps
        {
            SortKey = new Amazon.CDK.AWS.DynamoDB.Attribute
            {
                Name = "UserId",
                Type = AttributeType.STRING
            },
            IndexName = "Connection_UserId",
            ProjectionType = ProjectionType.ALL
        };
    }

    private Table CreateConnectionTable()
    {
        return new Table(this, "pub_sub_connection", new TableProps
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
    }
}