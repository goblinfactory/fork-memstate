using Goblinfactory.Azure.TableStorage;
using Memstate.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Memstate.Examples.AzureFunctions.TableStoreProvider
{
    public class TableStorageJournalReader : IJournalReader
    {
        private readonly ISerializer _serializer;

        private readonly string _partitionKey;
        TableStore<JournalEntity> _store;
        
        public TableStorageJournalReader(Connection connection, string tableName, string partitionKey)
        {
            _partitionKey = partitionKey;
            _store = TableStore<JournalEntity>.GetOrCreateStoreAsync(connection, tableName).ConfigureAwait(false).GetAwaiter().GetResult();
            var cfg = Config.Current;
            _serializer = cfg.CreateSerializer();

        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public IEnumerable<JournalRecord> GetRecords(long fromRecord = 0)
        {
            var records = _store.GetRows(_partitionKey).ConfigureAwait(false).GetAwaiter().GetResult();
            foreach (var record in records)
            {
                if (record.RecordNumber >= fromRecord)
                {
                    var cmd = (Command)_serializer.FromString(record.Command);
                    yield return new JournalRecord(record.RecordNumber, record.Written,cmd);
                }
            }

        }
    }
}
