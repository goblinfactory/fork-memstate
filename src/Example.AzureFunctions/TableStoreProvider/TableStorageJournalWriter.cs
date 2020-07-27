using Goblinfactory.Azure.TableStorage;
using Memstate.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Memstate.Examples.AzureFunctions.TableStoreProvider
{
    public class TableStorageJournalWriter : BatchingJournalWriter
    {
        string _partitionKey;
        TableStore<JournalEntity> _store;
        private readonly ISerializer _serializer;

        public long NextRecord { get; private set; }
        public TableStorageJournalWriter(Connection connection, string tableName, string partitionKey, long nextRecordNumber)
        {
            _partitionKey = partitionKey;
            _store = TableStore<JournalEntity>.GetOrCreateStoreAsync(connection, tableName).ConfigureAwait(false).GetAwaiter().GetResult();
            NextRecord = nextRecordNumber;
            var cfg = Config.Current;
            _serializer = cfg.CreateSerializer();
        } 

        public event TableStorageRecordsWrittenhandler RecordsWritten = delegate { };

        protected override void OnCommandBatch(IEnumerable<Command> commands)
        {
            //TODO: need to check the max size does not exceed 1MB.
            var batch = commands.Select(c => new JournalEntity(_partitionKey, NextRecord++, DateTimeOffset.Now, _serializer.ToString(c))).ToArray();
            _store.AddBatched(batch).ConfigureAwait(false).GetAwaiter().GetResult();
            RecordsWritten.Invoke(batch);
        }
    }

}
