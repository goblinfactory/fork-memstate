using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Memstate.Examples.AzureFunctions.TableStoreProvider
{
    [Serializable]
    public class JournalEntity : TableEntity
    {
        /// <summary>
        /// Sequential id of the record, always starts at 0
        /// </summary>
        public long RecordNumber { get; set; }

        /// <summary>
        /// Point in time when the record was written to the journal
        /// </summary>
        public DateTimeOffset Written { get; set; }

        public string Command { get; set; }

        public JournalEntity() { }
        public JournalEntity(string partitionKey, long recordNumber, DateTimeOffset written, string command)
        {
            PartitionKey = partitionKey;
            RowKey = recordNumber.ToString("0000000000");
            RecordNumber = recordNumber;
            Written = written;
            Command = command;
        }
    }
}