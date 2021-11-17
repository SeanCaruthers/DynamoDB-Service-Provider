using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DynamoTransferObjects
{
    public class DynamoDBTable
    {

        string TableName {get; set; }
        string PartitionAttribute {get; set;}
        string PartitionTypeString {get; set; }
        string RangeAttribute {get; set; }
        string RangeTypeString  {get; set; }

        /// <summary>
        /// ensures that table creation only occurs once
        /// </summary>
        static bool Initialized = false;

        public DynamoDBTable(
           Amazon.RegionEndpoint region,
           string TableName, 
           string PartitionAttribute, 
           string PartitionTypeString,
           string RangeAttribute,
           string RangeTypeString)
            { 
            
                this.TableName = TableName;
                this.PartitionAttribute = PartitionAttribute;
                this.PartitionTypeString = PartitionTypeString;
                this.RangeAttribute = RangeAttribute;
                this.RangeTypeString = RangeTypeString;

            // warning can be ignored.  We won't be making requests immediately after instantiating the object.
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            this.Initialize(region);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }


        public CreateTableRequest GetSchema()
        {
            return CreateSchema(
                TableName, 
                PartitionAttribute, 
                PartitionTypeString,
                RangeAttribute,
                RangeTypeString);
        }


        private async Task<bool> Initialize(Amazon.RegionEndpoint region)
        {

            if(Initialized){ return true; }

            var client = new AmazonDynamoDBClient();

            dynamic response = await client.ListTablesAsync();

            List<string> currentTables = response.TableNames;

            if(!currentTables.Contains(TableName)){
                try
                {
                    await client.CreateTableAsync(GetSchema());
                    Initialized = true;
                    
                }
                catch(Exception exc)
                {
                    Console.WriteLine($"Error in creating data table: {exc}");
                }
            }
            return Initialized;
        }

         /// <summary>
        /// Specifies the schema of the current table to be create
        /// 
        /// <para>Following documentation at <see cref=">docs.aws.amazon.com/amazondynamodb/latest/developerguide/LowLevelDotNetWorkingWithTables.html"/></para>
        /// <para>This is configured to Pay per request as opposed to using provisioned capacity (pay by time and instances)</para>
        /// </summary>
        /// <returns></returns>
        public static CreateTableRequest CreateSchema(
            string tableName, 
            string partitionAttribute, 
            string partitionTypeString,
            string rangeAttribute,
            string rangeTypeString)
        {
            return new CreateTableRequest
            {
                TableName = tableName,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        AttributeName = partitionAttribute,
                        AttributeType = partitionTypeString
                    },
                    new AttributeDefinition
                    {
                        AttributeName = rangeAttribute,
                        AttributeType = rangeTypeString
                    }
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = partitionAttribute,
                        KeyType = "HASH"

                    },
                    new KeySchemaElement
                    {
                        AttributeName = rangeAttribute,
                        KeyType = "RANGE"
                    },

                },
                BillingMode = BillingMode.PAY_PER_REQUEST

            };
        }
    }
}
