using Memstate.Configuration;
using System;
using System.Collections.Generic;

namespace Memstate.Examples.AzureFunctions.TableStoreProvider
{
    internal class FileJournalSubscriptionSource : IJournalSubscriptionSource
    {
        private readonly ISerializer _serializer;

        private readonly Dictionary<Guid, JournalSubscription> _subscriptions;

        private readonly TableStorageJournalWriter _journalWriter;

        public FileJournalSubscriptionSource(TableStorageJournalWriter journalWriter)
        {
            var cfg = Config.Current;
            _serializer = cfg.CreateSerializer();
            _journalWriter = journalWriter;
            _subscriptions = new Dictionary<Guid, JournalSubscription>();
            _journalWriter.RecordsWritten += OnRecordsWritten;
        }

        public IJournalSubscription Subscribe(long from, Action<JournalRecord> handler)
        {
            if (from != _journalWriter.NextRecord)
            {
                throw new NotSupportedException("CatchupSubscriptions are not supported by this FileStorageProvider");
            }

            var sub = new JournalSubscription(handler, from, OnDisposed);

            lock (_subscriptions)
            {
                _subscriptions.Add(sub.Id, sub);
            }

            return sub;
        }

        public void Dispose()
        {
            _journalWriter.RecordsWritten -= OnRecordsWritten;
        }

        private void OnRecordsWritten(JournalEntity[] records)
        {
            lock (_subscriptions)
            {
                foreach (var subscription in _subscriptions.Values)
                {
                    foreach (var record in records)
                    {
                        var cmd = (Command)_serializer.FromString(record.Command);
                        var rec = new JournalRecord(record.RecordNumber, record.Written, cmd);
                        subscription.Handle(rec);
                    }
                }
            }
        }

        private void OnDisposed(JournalSubscription subscription)
        {
            lock (_subscriptions)
            {
                _subscriptions.Remove(subscription.Id);
            }
        }
    }
}