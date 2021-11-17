using Amazon.DynamoDBv2.DataModel;
using DynamoDBInterfaces;

namespace DynamoTransferObjects
{
    /// <summary>
    /// 
    /// Everything that an indexed dynamodb object needs.
    /// 
    /// Inherited classes should specify the DynamoDBTable by adding 
    /// the attribute DynamoDBTable to the class definition as well as
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
    /// public class CLASS_NAME : DynamoDBIndexedTransferObjectData<IndexTableEntry, CLASS_NAME, Partition, Range> {
    ///
    ///    [DynamoDBHashKey("PARTITION-KEY-ATTRIBUTE-NAME")]
    ////   public override Partition PartitionKey { get; set; }

    ///    [DynamoDBRangeKey("RANGE-KEY-ATTRIBUTE-NAME")]
    ///    public override Range RangeKey { get; set; }
    ///   
    ///   }    
    /// 
    /// </summary>
    public abstract class DynamoDBIndexedTransferObject<IndexTableEntry, DataTableEntry, Partition, Range> : 
        DynamoDBTransferObject<Partition, Range>,
        IHasDynamoKeys<Partition, Range>, IConvertsToIndex<IndexTableEntry, DataTableEntry>
        where DataTableEntry : IAmDataTable
        where IndexTableEntry: IAmIndexTable, new()
    { 
        

        public IndexTableEntry ToIndex()
        {
            dynamic index = new IndexTableEntry();
            index.PartitionKey = GetPartitionKey();
            index.RangeKey = GetRangeKey();
            return index;
        }
    }

    public abstract class DynamoDBIndexedTransferObjectIndex<Partition, Range> : 
        DynamoDBTransferObject<Partition, Range>, 
        IAmIndexTable{ }
    
}
