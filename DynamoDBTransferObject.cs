using Amazon.DynamoDBv2.DataModel;
using DynamoDBInterfaces;

namespace DynamoTransferObjects
{
    /// <summary>
    /// 
    /// Everything that a dynamodb object needs.
    /// 
    /// Inherited classes should specify the DynamoDBTable by adding 
    /// the attribute DynamoDBTAble to the class definition as well as
    /// getters and setters to the PartitionKey and RangeKey variables.
    /// 
    /// The purpose of this is to hide the internal implementation details of the DynamoDB CRUD operations
    /// so that we can just focus upon defining the data transfer objects
    /// 
    /// ex: 
    /// using Partition = PARTITION_KEY_TYPE
    /// using Range = Range_KEY_TYPE
    /// 
    /// attribute DynamoDBTable("DYNAMODB-TABLE-NAME")
    /// public class CLASS_NAME : DynamoTransferObjects.DynamoDBBaseTransferObject<Partition, Range> {
    ///
    ///    [DynamoDBHashKey("PARTITION-KEY-ATTRIBUTE-NAME")]
    ////   public override Partition PartitionKey { get; set; }

    ///    [DynamoDBRangeKey("RANGE-KEY-ATTRIBUTE-NAME")]
    ///    public override Range RangeKey { get; set; }
    ///   
    ///   }    
    /// 
    /// </summary>
    public abstract class DynamoDBTransferObject<Partition, Range> : IHasDynamoKeys<Partition, Range>
    {
        public abstract Partition PartitionKey { get; set; }
        public abstract Range RangeKey { get; set; }


        /// <summary>
        /// Ensures that deletes and updates fail if data versions are not consistent (File lock)
        /// </summary>
        [DynamoDBVersion]
        public int? VersionNumber { get; set; }

        public Partition GetPartitionKey()
        {
            return PartitionKey;
        }

        public Range GetRangeKey()
        {
            return RangeKey;
        }
    }
}
