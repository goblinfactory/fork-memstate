using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Goblinfactory.Azure.TableStorage
{
    public interface ITableStore<T> where T : ITableEntity 
    {
        Task InsertOrMerge(T item);
        Task AddBatched(IEnumerable<T> items);
        Task<T> Get(string id);
        Task<T> Get(string partitionKey, string rowKey);
        IAsyncEnumerable<string> GetPartitionKeyStream();  
        Task<LinkedList<string>> GetPartitionKeys(); 
        Task<LinkedList<T>> GetRows(string partitionKey); 
        IAsyncEnumerable<T> GetRowsStream(string partitionKey);

    }
}