namespace Goblinfactory.Azure.TableStorage
{

    public class DevConnection : Connection
    {
        public DevConnection() : base() {  }
    }
    public class Connection
    {
        public string Prefix { get; } = "";
        public string ConnectionString { get; } = null;
        public bool UseDevStorageAccount { get; } = false;

        public Connection(string prefix, string connection)
        {
            Prefix = prefix;
            ConnectionString = connection;
        }
        /// <summary>
        ///  create a connection that uses the local 
        /// </summary>
        public Connection(string prefix)
        {
            UseDevStorageAccount = true;
            Prefix = prefix;
        }

        public Connection()
        {
            UseDevStorageAccount = true;
            Prefix = "";
        }
    }
}