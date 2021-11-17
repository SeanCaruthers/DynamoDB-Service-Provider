
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DynamoDBInterfaces
{
    
    #region interfaces
    /// <summary>
    /// 
    /// <para>
    ///     Implementation Interface for the object used with the DynamoDB Object Persistence Model
    /// </para>
    /// 
    /// 
    /// </summary>
    /// <typeparam name="PartitionKey"> the attribute type used to partition the table</typeparam>
    /// <typeparam name="RangeKey"> the attribute type used to Range and query the table</typeparam>
    public interface IHasDynamoKeys<PartitionKey, RangeKey>{ 
        public PartitionKey GetPartitionKey();
        public RangeKey GetRangeKey();
    }


    

    /// <summary>
    /// 
    /// <para>
    ///     Basic interface for dynamoDB CRUD operations for a simple table
    /// </para>
    /// 
    /// </summary>
    /// <typeparam name="TableEntry">the object used to represent the table</typeparam>
    /// <typeparam name="Partition"> the attribute type used to partition the table </typeparam>
    /// <typeparam name="Key"> the attribute type used to Range and query the table </typeparam>
    public interface IHasDynamoCRUD<TableEntry, Partition, Key>
    {
        public Task<Key> Create(TableEntry instance);
        public Task<TableEntry> Read(Partition partition, Key key);
        public Task<List<TableEntry>> Read(Partition partition);
        public Task Update(TableEntry updatedInstance);
        public Task Delete(Partition partition, Key key);
    }



    /// <summary>
    /// Interface to show that the model represents a real data table
    /// </summary>
    public interface IAmDataTable{ }

    /// <summary>
    /// Interface to show that the model represents an index table
    /// </summary>
    public interface IAmIndexTable{ }

    /// <summary>
    /// Interface for IDataTables that must convert to index
    /// </summary>
    /// <typeparam name="IndexTableEntry"></typeparam>
    public interface IConvertsToIndex<IndexTableEntry, DataTableEntry>
    {
        public IndexTableEntry ToIndex();
    }

    /// <summary>
    /// 
    /// <para>
    ///     Basic interface for dynamoDB CRUD operations for an indexed table
    ///     The read all opertation should go through the IndexTable
    ///     The create and delete functions should provide some transactional mechanism to maintain data consistency between the index and data tables.
    /// </para>
    /// 
    /// </summary>
    /// <typepara name="IndexTableEntry">the object used to represent the table index</typepara>
    /// <typeparam name="DataTableEntry">the object used to represent the table</typeparam>
    /// <typeparam name="Partition"> the attribute type used to partition the table </typeparam>
    /// <typeparam name="Key"> the attribute type used to Range and query the table </typeparam>
    public interface IHasIndexedDynamoCRUD<IndexTableEntry, DataTableEntry, Partition, Key>
    {
        public Task<Key> Create(DataTableEntry instance);
        public Task<DataTableEntry> Read(Partition partition, Key key);
        public Task<List<IndexTableEntry>> Read(Partition partition);
        public Task Update(DataTableEntry updatedInstance);
        public Task Delete(Partition partition, Key key);
    }
    #endregion
  
}