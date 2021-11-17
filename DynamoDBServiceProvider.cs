using Amazon;
using Amazon.DynamoDBv2;


namespace DynamoDB
{
    /// <summary>
    /// 
    /// <para>
    ///     Base provider for DynamoDB service.  Is responsible for holding the DBClient 
    /// </para>
    /// 
    /// </summary>
    public class DynamoDBServiceProvider
    { 
        public AmazonDynamoDBClient client;

  
        public DynamoDBServiceProvider(RegionEndpoint tableRegion)
        {
            AmazonDynamoDBConfig regionConfig = new AmazonDynamoDBConfig();
            regionConfig.RegionEndpoint = tableRegion;
            client ??= new AmazonDynamoDBClient(regionConfig);
        }

    }
}
