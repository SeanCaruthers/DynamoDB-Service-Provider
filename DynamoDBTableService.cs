using Amazon.DynamoDBv2.DataModel;
using DynamoDBInterfaces;
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;


namespace DynamoDB
{

   

    #region DynamoDBTableService
    /// <summary>
 
    /// <para>
    ///     Dynamo db table service for objects that follow the dynamodb .NET Object Persistence Model
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
    public abstract class DynamoDBTableService<TableEntry, PartitionKey, RangeKey> : IHasDynamoCRUD<TableEntry, PartitionKey, RangeKey>
        where TableEntry : IHasDynamoKeys<PartitionKey, RangeKey>
    {

        #region data members
        internal DynamoDBContext _db = null;
        #endregion

        #region constructor
        /// <summary>
        /// The dynamo db constructor must be passed an amazon RegionEndpoint to correctly launch the DynamoDB service.
        /// </summary>
        /// <param name="region">The region that service is taking place in.</param>
        public DynamoDBTableService(Amazon.RegionEndpoint region)
        {

            DynamoDBServiceProvider serviceProvider = new DynamoDBServiceProvider(region);
            if(null == _db)
            {
                _db  = new DynamoDBContext(serviceProvider.client);
            }
        
        }
        #endregion

        #region methods
        public async Task<RangeKey> Create(TableEntry entry)
        { 
            try
            {
                await _db.SaveAsync<TableEntry>(entry);
            }
            catch (Exception exc)
            {
                System.Console.WriteLine($"Dynamo Create Error: {exc.Message}");
                ExceptionDispatchInfo.Capture(exc).Throw();
            }
            return entry.GetRangeKey();
        }


        public virtual async Task<TableEntry> Read(PartitionKey partition, RangeKey key) 
         {
            TableEntry entry = default;
            try
            {
                entry = await _db.LoadAsync<TableEntry>(partition, key);   
            }
            catch (Exception e)
            {
                System.Console.WriteLine($"Dynamo Read Error: {e.Message}");
                ExceptionDispatchInfo.Capture(e).Throw();
            }
            return entry;
        }


         public async Task<List<TableEntry>> Read(PartitionKey key) 
         {
            List<TableEntry> entries = null;
            try
            {
                AsyncSearch<TableEntry> query = _db.QueryAsync<TableEntry>(key);
                entries = await query.GetRemainingAsync();
            }
            catch (Exception e)
            {
                System.Console.WriteLine($"Dynamo Write Error: {e.Message}");
                ExceptionDispatchInfo.Capture(e).Throw();
            }
            return entries;
        }


        public async Task Delete(PartitionKey partition, RangeKey key) 
        {
            await _db.DeleteAsync<TableEntry>(partition, key);
        }
        #endregion


        #region in progress
        
        #endregion
        #region TODO
       

        
        public async Task Update(TableEntry updatedInstance)
        {
            throw new Exception("Update not yet implemented!");
        }

        
        #endregion

    }
    #endregion


}
