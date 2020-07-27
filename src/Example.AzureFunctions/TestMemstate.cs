using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Memstate.Docs.GettingStarted.QuickStart.Queries;

namespace Memstate.Examples.AzureFunctions
{
    public static class TestMemstate
    {
        [FunctionName("customers")]
        public static async Task<IActionResult> Customers([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation($"fetching customers");
            var customers = await Service.Engine.Execute(new GetCustomers());
            return new JsonResult(customers);
        }

        [FunctionName("customers/top10")]
        public static async Task<IActionResult> CustomersTop10([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation($"fetching customers");
            var customers = await Service.Engine.Execute(new GetCustomers());
            return new JsonResult(customers);
        }

    }
}
