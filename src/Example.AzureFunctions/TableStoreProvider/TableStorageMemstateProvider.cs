using Goblinfactory.Azure.TableStorage;
using Memstate.Configuration;
using System;
using System.Threading.Tasks;

namespace Memstate.Examples.AzureFunctions.TableStoreProvider
{

    public class TableStorageMemstateProvider : StorageProvider
    {
        private Connection _connection;
        private string _tableName;
        private string _partitionKey;
        private TableStorageJournalWriter _currentWriter;

        public TableStorageMemstateProvider(Connection connection, string tableName, string partitionKey)
        {
            _connection = connection;
            _tableName = tableName;
            _partitionKey = partitionKey;
        }

        public override IJournalReader CreateJournalReader()
        {
            return new TableStorageJournalReader(_connection, _tableName, _partitionKey );
        }

        public override bool SupportsCatchupSubscriptions()
        {
            return false;
        }
        public override IJournalWriter CreateJournalWriter(long nextRecordNumber)
        {
             _currentWriter = new TableStorageJournalWriter(_connection, _tableName, _partitionKey, nextRecordNumber);
            return _currentWriter;
        }

        public override IJournalSubscriptionSource CreateJournalSubscriptionSource()
        {
            if (_currentWriter == null)
            {
                throw new InvalidOperationException("Cannot create subscriptionsource");
            }

            return new FileJournalSubscriptionSource(_currentWriter);
        }

        public static async Task<Engine<T>> StartEngine<T>(Connection connection, Action<string> info, string tableName, string partitionKey) where T : class
        {
            info("creating/getting memstate current configuration");
            var config = Config.Current;
            info("-----");
            info(config.ToString());
            info("-----");
            config.SerializerName = "NewtonSoft.Json";
            info("creating azure table storage provider for [demoaccounts], with partition key ='goblinfactory'");
            var provider = new TableStorageMemstateProvider(connection, tableName, partitionKey);
            config.SetStorageProvider(provider);
            info("starting memstate engine");
            Engine<T> engine = await Engine.Start<T>();
            return engine;
        }
    }
}
