using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Threading.Tasks;
using MoreLinq;
using Memstate;
using Memstate.Configuration;
using Memstate.Examples.AzureFunctions.TableStoreProvider;

namespace Goblinfactory.Azure.TableStorage
{
    //TODO: add in optimistic concurrency checks! etag support.

    public class TableStore<T> : ITableStore<T> where T : ITableEntity, new()
    {
        // NB! I need to investigate retry policy, otherwise I'm going to 
        // be in for some interesting debugging sessions. see new TableRequestOptions.
        // add in Polly.
        // ---------------------------------------------
        public static async Task<TableStore<T>> GetOrCreateStoreAsync(Connection connection, string tableName = "") 
        {
            try
            {
                var cloud = connection.UseDevStorageAccount ? CloudStorageAccount.DevelopmentStorageAccount : CloudStorageAccount.Parse(connection.ConnectionString);
                var client = cloud.CreateCloudTableClient();
                var name = !string.IsNullOrWhiteSpace(tableName)
                    ? tableName.ValidateTableStorageName()
                    : $"{connection.Prefix}{typeof(T).Name.Replace("Entity", "")}".ValidateTableStorageName();
                var table = client.GetTableReference(name);
                await table.CreateIfNotExistsAsync();
                return new TableStore<T>(table);
            }
            catch (StorageException se)
            {
                //if(connection.UseDevStorageAccount)
                // need to log info that you may have forgotten to start your local azure storage emulator.
                // Microsoft.WindowsAzure.Storage.StorageException: 'No connection could be made because the target machine actively refused it.'                
                throw;
            }
        }


        private readonly CloudTable _table;
        private TableStore(CloudTable table)
        {
            _table = table;
        }

        public async Task InsertOrMerge(T item)
        {
            var insert = TableOperation.InsertOrMerge(item);
            await _table.ExecuteAsync(insert);
        }
        public static bool Merge = true;
        public async Task AddBatched(IEnumerable<T> items)
        {
            var batches = items.Batch(100);
            foreach(var batch in batches)
            {
                var batchOperation = new TableBatchOperation();
                foreach (var item in batch)
                {
                    // speed test using merge vs not i
                    if (Merge)
                        batchOperation.InsertOrMerge(item);
                    else
                        batchOperation.Insert(item);
                }
                await _table.ExecuteBatchAsync(batchOperation);
            }
        }
        public async Task<T> Get(string partitionKey, string rowKey)
        {
            var retrieve = TableOperation.Retrieve<T>(partitionKey, rowKey);
            var result = await _table.ExecuteAsync(retrieve);
            return (T)result.Result;
        }

        public async Task<T> Get(string partitionKey)
        {
            var retrieve = TableOperation.Retrieve<T>(partitionKey, null);
            var result = await _table.ExecuteAsync(retrieve);
            return (T)result.Result;
        }

        public async IAsyncEnumerable<string> GetPartitionKeyStream()
        {
            // https://github.com/Azure-Samples/storage-table-dotnet-getting-started
            // https://serversncode.com/dotnet-c-sharp-with-azure-table-storage-inserting-retrieving-records/

            var query = new TableQuery(); // no filter, return all records
            TableContinuationToken token = null;
            do
            {
                var segment = await _table.ExecuteQuerySegmentedAsync(query, token);
                token = segment.ContinuationToken;
                foreach (var result in segment.Results)
                {
                    yield return result.PartitionKey;
                }
            } while (token != null);
        }

        public async Task<LinkedList<string>> GetPartitionKeys()
        {
            var query = new TableQuery(); // no filter, return all records
            TableContinuationToken token = null;
            var keys = new LinkedList<string>();
            do
            {
                var segment = await _table.ExecuteQuerySegmentedAsync(query, token);
                token = segment.ContinuationToken;
                foreach (var result in segment.Results)
                {
                    keys.AddLast(result.PartitionKey);
                }
            } while (token != null);
            return keys;
        }

        public async Task<LinkedList<T>> GetRows(string partitionKey)
        {
            var query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
            var items = new LinkedList<T>();
            TableContinuationToken token = null;
            do
            {
                var segment = await _table.ExecuteQuerySegmentedAsync(query, token);
                token = segment.ContinuationToken;
                if (segment?.Results != null) foreach (var result in segment.Results)
                    {
                        items.AddLast((T)result);
                    }
            } while (token != null);
            return items;
        }

        //TODO; update this to include an overload that allows you you specify the properties you want to bring back to reduce bandwidth usage and reduce costs.
        // e.g. select a.first, a.last instead of select * from 
        public async IAsyncEnumerable<T> GetRowsStream(string partitionKey)
        {
            var query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
            TableContinuationToken token = null;
            do
            {
                var segment = await _table.ExecuteQuerySegmentedAsync(query, token);
                token = segment.ContinuationToken;
                if (segment?.Results != null) foreach (var result in segment.Results)
                    {
                        yield return (T)result;
                    }
            } while (token != null);

        }



    }
}