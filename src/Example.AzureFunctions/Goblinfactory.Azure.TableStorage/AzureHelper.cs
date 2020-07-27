using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Goblinfactory.Azure.TableStorage
{
    public static class AzureHelper
    {
        public static async IAsyncEnumerable<T> GetAllByPartitionKeyStream<T>(this CloudTable table, string partitionKey) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
            TableContinuationToken token = null;
            do
            {
                var foo = await table.ExecuteQuerySegmentedAsync(query, token);
                token = foo.ContinuationToken;
                if (foo?.Results != null) foreach (var result in foo.Results)
                    {
                        yield return (T)result;
                    }
            } while (token != null);
        }
        public static async Task<List<T>> GetAllByPartitionKey<T>(this CloudTable table, string partitionKey) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
            var items = new List<T>();
            TableContinuationToken token = null;
            do
            {
                var foo = await table.ExecuteQuerySegmentedAsync(query, token);
                token = foo.ContinuationToken;
                if (foo?.Results != null) foreach (var result in foo.Results)
                    {
                        items.Add(result);
                    }
            } while (token != null);
            return items;
        }
    }
}
