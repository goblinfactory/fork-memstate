using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Memstate.Docs.GettingStarted.QuickStart;
using System.Threading.Tasks;
using Memstate.Configuration;
using Goblinfactory.Azure.TableStorage;
using System.IO;
using Memstate.Examples.AzureFunctions.TableStoreProvider;

namespace Memstate.Examples.AzureFunctions
{
    public static class Service
    {
        private static object _locker = new object();
        private static bool _running = false;
        private static Engine<LoyaltyDB> _engine;
        
        // service start errors is only ever non null if there were errors during startup where we don't have access to a logger.
        private static ConcurrentBag<string> _serviceStartErrors = null;
        public static Engine<LoyaltyDB> Model
        {
            get
            {
                lock (_locker)
                {
                    var startLog = new List<string>();
                    try
                    {
                        Action<string> Info = text => startLog.Add(text);
                        if (_running) return _engine;
                        var connection = GetConnect();
                        _engine =  TableStorageMemstateProvider.StartEngine(true, Info).GetAwaiter().GetResult();
                        _running = true;
                    }
                    catch(Exception ex)
                    {
                        // only include the startup log if something goes wrong to save on keeping static memory around.
                        foreach (var text in startLog) _serviceStartErrors.Add($"log:{text}");
                        _serviceStartErrors.Add($"message:{ex.Message}");
                        _serviceStartErrors.Add($"error:{ex}");
                    }
                }
                return _engine;
            }
        }

        public class Status
        {
            public Status(bool hasErrors, string[] engineStartLog)
            {
                HasErrors = hasErrors;
                EngineStartLog = engineStartLog;
            }
            public string[] EngineStartLog { get; set; }
            public bool HasErrors { get; set; }
        }


        [FunctionName("status")]
        public static IActionResult GetStatus([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation($"get status");
                var status = new Status(_serviceStartErrors!=null, _serviceStartErrors?.ToArray() ?? new string[] { });
                return new JsonResult(status);
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                throw;
            }
        }

        static Service()
        {
            // safety net in case engine not stopped, so that we force a flush, in case that's needed, I dont think this code can ever run
            // the only situation would be if azure 5 minute timeout occurs before memstate flushed any outstanding records.
            // considering the design of memstate I dont think that's possible.
            AppDomain.CurrentDomain.ProcessExit += (s, e) => {
                if (_running)
                {
                    try
                    {
                        _engine.DisposeAsync().GetAwaiter().GetResult();
                    } catch { }
                }
            };
        }

        private static Connection GetConnect()
        {
            // set to true to deploy to azure, leave as is to test locally using AzureStorageEmulator
            bool useLocalAzureFunctionStorageEmulator = true;
            if (useLocalAzureFunctionStorageEmulator) return new DevConnection();
            var prefix = "memstateAzure";
            // TODO: check if code below windows specific? linux compatible?
            var connString = File.ReadAllText(Environment.ExpandEnvironmentVariables("%APPDATA%/Goblinfactory.Connections/AZURE_CLOUDSTORAGE.CONNECTION"));
            var conn = new Connection(prefix, connString);
            return conn;
        }

    }
}
