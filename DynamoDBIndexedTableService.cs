using Amazon.DynamoDBv2.DataModel;
using DynamoDBInterfaces;
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;


namespace DynamoDB
{

   

    #region DynamoDBIndexedTableService
    /// <summary>
 
    /// <para>
    ///     Dynamo db indexed table service for objects that follow the dynamodb .NET Object Persistence Model
    /// </para>
    /// 
    /// <para>
    ///      Is used to provide the interactions with the dynamodb client
    /// 
    ///      this class should be inherited from to provide the data specifics to the client
    /// </para>
    ///      
    /// <para>
    ///      Following Documentation at:
    /// 
    ///     <see cref="docs.aws.amazon.com/amazondynamodb/latest/developerguide/DotNetSDKHighLevel.html "/>  
    /// </para>
    /// 
    /// 
    /// </summary>
    /// <typeparam name="TableEntry">The Table entry type is the class name of the object that corresponds with the the DynamoDB table</typeparam>
    /// <typeparam name="PartitionKey">The partition key is the value used to partition the DynamoDB table when querying</typeparam>
    /// <typeparam name="RangeKey">The Range key is used to further Range the table for querying</typeparam>
    public abstract class DynamoDBIndexedTableService<IndexTableEntry, DataTableEntry, PartitionKey, RangeKey> : 

        IHasIndexedDynamoCRUD<IndexTableEntry, DataTableEntry, PartitionKey, RangeKey>

        where IndexTableEntry : IHasDynamoKeys<PartitionKey, RangeKey>, IAmIndexTable

        where DataTableEntry : IHasDynamoKeys<PartitionKey, RangeKey>, IAmDataTable, IConvertsToIndex<IndexTableEntry, DataTableEntry>

    {

        #region data members
        internal DynamoDBContext _db = null;
        #endregion

        #region constructor
        /// <summary>
        /// The dynamo db constructor must be passed an amazon RegionEndpoint to correctly launch the DynamoDB service.
        /// </summary>
        /// <param name="region">The region that service is taking place in.</param>
        public DynamoDBIndexedTableService(Amazon.RegionEndpoint region)
        {

            DynamoDBServiceProvider serviceProvider = new DynamoDBServiceProvider(region);
            if(null == _db)
            {
                _db  = new DynamoDBContext(serviceProvider.client);
            }
        
        }
        #endregion

        #region methods
        /// <summary>
        /// <para>
        /// Create an index and data entry.  
        /// Insert should remain transactional to maintain consistency between indexes and data
        /// rollback index if data fails to insert but index was created.
        /// </para>
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public async Task<RangeKey> Create(DataTableEntry entry)
        { 
            int retry = 2;
            bool IndexInserted = false;
            IndexTableEntry index = entry.ToIndex();
            while(retry > -1)
            {
                bool lastIteration = (retry == 0);

                if (!IndexInserted)
                {
                    try
                    {
                    
                        await _db.SaveAsync(index);
                        IndexInserted = true;
                    
                    }
                    catch (Exception exc)
                    { 
                        System.Diagnostics.Debug.Write($"{exc}");
                        if(lastIteration)
                        {
                            System.Console.WriteLine($"Unable to create index entry: {exc.Message}");
                            ExceptionDispatchInfo.Capture(exc).Throw();
                        }
                    }
                }

                if (IndexInserted)
                {
                    try
                    {
                        await _db.SaveAsync(entry);
                        return entry.GetRangeKey();
                    }
                    catch (Exception exc)
                    {
                        System.Diagnostics.Debug.Write($"{exc}");
                        if(lastIteration)
                        {

                            try
                            {
                                await IndexDelete(index.GetPartitionKey(), index.GetRangeKey());
                            }
                            catch (Exception exc2)
                            {
                                System.Diagnostics.Debug.Write($"{exc2}");
                                throw new Exception($"Error state, Data added to index, but not table: {exc2.Message}");
                            }
                           
                        }
                    }
                }
                retry -= 1;
            }
            throw new Exception("unable to create entry!");
        }


        /// <summary>
        ///  Read the data values of a specific index
        /// </summary>
        /// <param name="partition"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<DataTableEntry> Read(PartitionKey partition, RangeKey key) 
         {
            DataTableEntry entry = default;
            try
            {
                entry = await _db.LoadAsync<DataTableEntry>(partition, key);   
            }
            catch (Exception exc)
            {
                System.Console.WriteLine($"Dynamo Indexed Read Error: {exc.Message}");
                ExceptionDispatchInfo.Capture(exc).Throw();
            }
            return entry;
        }


        /// <summary>
        /// Read the index values from the Index table
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
         public async Task<List<IndexTableEntry>> Read(PartitionKey key) 
         {
            List<IndexTableEntry> entries = null;
            try
            {
                AsyncSearch<IndexTableEntry> query = _db.QueryAsync<IndexTableEntry>(key);
                entries = await query.GetRemainingAsync();
            }
            catch (Exception exc)
            {
                System.Console.WriteLine($"Dynamo Indexed Write Error: {exc.Message}");
                ExceptionDispatchInfo.Capture(exc).Throw();
            }
            return entries;
        }


        /// <summary>
        /// delete an index and data entry for an index data item
        /// delete should be transactional and will attempt to  delete again if error occurs
        /// </summary>
        /// <param name="partition"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task Delete(PartitionKey partition, RangeKey key) 
        {
            int retry = 2;
            bool IndexDeleted = false;
            while (retry > 0)
            {
                try
                {
                    if (!IndexDeleted)
                    {
                        await _db.DeleteAsync<IndexTableEntry>(partition, key);
                        IndexDeleted = true;
                    }
                    
                    await _db.DeleteAsync<DataTableEntry>(partition, key);
                    return;
                }
                catch (Exception exc){
                    if (retry == 0)
                    {
                        ExceptionDispatchInfo.Capture(exc).Throw();
                    }
 
                    if (!IndexDeleted)
                    {
                        System.Console.WriteLine($"Dynamo Indexed Delete Error - unable to delete index: {exc.Message}");
                    }
                    else
                    {
                        System.Console.WriteLine($"Dynamo Indexed Delete Error - unable to delete data entry {partition} {key}: {exc.Message}");
                    }
                 
                }
                retry -= 1;
            }

            
            
        }

        /// <summary>
        /// delete an index 
        /// </summary>
        /// <param name="partition"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private async Task IndexDelete(PartitionKey partition, RangeKey key) 
        {
            int retry = 2;
            while(retry > 0)
            {
                try
                {
                    await _db.DeleteAsync<IndexTableEntry>(partition, key);
                    return;
                }
                catch(Exception exc)
                {
                    Console.WriteLine($"Error in deleting index: {exc}");
                }
                retry -= 1;
            }
            throw new Exception($"Unable to delete index!!!!!!!!!!!!!!!!!!");
        }
            
        #endregion


        #region in progress
        
        #endregion
        #region TODO
       

        
        public async Task Update(DataTableEntry updatedInstance)
        {
            throw new Exception("Update not yet implemented!");
        }

        
        #endregion

    }
    #endregion


}
