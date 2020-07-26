using System;
using System.IO;
using System.Threading.Tasks;
using Memstate.Azure;
using Memstate.Configuration;
using Memstate.Docs.GettingStarted.QuickStart;
using Memstate.Docs.GettingStarted.QuickStart.Commands;
using Memstate.Docs.GettingStarted.QuickStart.Queries;
using Microsoft.Azure.Cosmos.Table;
using NUnit.Framework;

namespace Memstate.Docs.GettingStarted.AzureTableStorage
{
    public class HowToUseTheStorageProvider
    {
        // TODO: 
        // using the AzureTable storage connection string do the full setup and teardown of the azure tables.
        // will help someone know exactly what they should be looking for in azure.
        

        [Test]
         public async Task Simple_end_to_end_getting_started_using_default_wire_serializer_with_filesytem_storage()
        {
            Print("GIVEN I start a new Memstate engine for a LoyaltyDB");

            var config = Config.CreateDefault();
            var cloudTable = await GetCloudTableAsync("memstateDemoLoyalty");  //see helper method below
            config.UseAzureTableStorage(cloudTable);                //here is where the magic happens

            //The provider will use the StreamName from EngineSettings, the default is "memstate":
            config.GetSettings<EngineSettings>().StreamName = "my-stream";

            //That's it, you should be all set. Start your engines!
            var engine = await Engine.Start<LoyaltyDB>(config);
            
            Print("AND I initialize the database with 20 customers, each with 10 loyalty points");
            for (int i = 0; i < 20; i++)
            {
                await engine.Execute(new InitCustomer(i + 1, 10));
            }

            Print("THEN a journal table should now exist in Azure");
            // this is coming, for now, please manually check this.

            Print("WHEN customer 5 and customer 12 each earn 190 and 290 loyalty points respectively");
            var c1 = await engine.Execute(new EarnPoints(5, 190));
            var c2 = await engine.Execute(new EarnPoints(12, 290));

            Print("THEN the balance for them will have increased to 200 and 300 loyalty points for customer 5 and 12 respectively");
            Assert.AreEqual(200, c1.LoyaltyPointBalance);
            Assert.AreEqual(300, c2.LoyaltyPointBalance);

            await engine.DisposeAsync();
        }



        [Test]
        public async Task WHEN_starting_up_another_engine_THEN_the_model_state_should_be_restored_and_where_we_left_it()
        {
            // manually run this test AFTER having manually running the test above at least once.
            // before running these tests, please manually delete the table in azure, 
            // otherwise the balance will be incremented due to running the tests above.

            var config = Config.CreateDefault();
            var cloudTable =await GetCloudTableAsync("memstateDemoLoyalty");  
            config.UseAzureTableStorage(cloudTable);                
            config.GetSettings<EngineSettings>().StreamName = "my-stream";
            var engine = await Engine.Start<LoyaltyDB>(config);

            Print("WHEN I start up another engine");
            engine = await Engine.Start<LoyaltyDB>(config);

            Print("THEN the entire journal at this point should immediately replay all the journaled commands saved to the filesystem");
            var allCustomers = await engine.Execute(new GetCustomers());

            Print("AND the database should be restored to the exact same state it was after the last command was executed");
            Assert.AreEqual(20, allCustomers.Count);
            Assert.AreEqual(200, allCustomers[5].LoyaltyPointBalance);
            Assert.AreEqual(300, allCustomers[12].LoyaltyPointBalance);
            await engine.DisposeAsync();
        }

        //This is just basic Azure SDK configuration
        private static async Task<CloudTable> GetCloudTableAsync(string tableName)
        {
            //you can find the connection string in the Azure Portal.
            //This example uses environment variables which is one way to do it
            var connectionString = File.ReadAllText(Environment.ExpandEnvironmentVariables("%APPDATA%/Goblinfactory.Connections/AZURE_CLOUDSTORAGE.CONNECTION"));

            if (String.IsNullOrEmpty(connectionString))
                throw new Exception("AZURE_CLOUDSTORAGE_CONNECTION env variable not set");
            var account = CloudStorageAccount.Parse(connectionString);
            var client = account.CreateCloudTableClient();
            var table = client.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();
            return table;
        }


        private void Print(string text)
        {
            Console.WriteLine(text);
        }
    }
}
