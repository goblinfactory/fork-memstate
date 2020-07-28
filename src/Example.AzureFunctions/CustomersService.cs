using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Memstate.Docs.GettingStarted.QuickStart.Queries;
using System.IO;
using Newtonsoft.Json;
using Memstate.Docs.GettingStarted.QuickStart.Commands;
using Memstate.Docs.GettingStarted.QuickStart;

namespace Memstate.Examples.AzureFunctions
{
    public static class CustomersService
    {
        [FunctionName("customers")]
        public static async Task<IActionResult> Customers([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, ILogger log)
        {
            var customers = await Service.Engine.Execute(log, new GetCustomers());
            return new JsonResult(customers);
        }

        [FunctionName("customers/top10")]
        public static async Task<IActionResult> CustomersTop10([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, ILogger log)
        {
            var customers = await Service.Engine.Execute(log, new Top10Customers());
            return new JsonResult(customers);
        }

        [FunctionName("customers/{id}")]
        public static async Task<IActionResult> EarnPoints([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            EarnPoints earnPointsCmd = JsonConvert.DeserializeObject<EarnPoints>(requestBody);
            Customer customer = await Service.Engine.Execute(log, earnPointsCmd);
            return new OkObjectResult(customer);
        }

        [FunctionName("customers/{id}")]
        public static async Task<IActionResult> InitCustomer([HttpTrigger(AuthorizationLevel.Function, "put", Route = null)] HttpRequest req, ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            InitCustomer initCustomer = JsonConvert.DeserializeObject<InitCustomer>(requestBody);
            Customer customer = await Service.Engine.Execute(log, initCustomer);
            return new OkObjectResult(customer);
        }

        [FunctionName("customers/{id}")]
        public static async Task<IActionResult> SpendPoints([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            SpendPoints points = JsonConvert.DeserializeObject<SpendPoints>(requestBody);
            Customer customer = await Service.Engine.Execute(log, points);
            return new OkObjectResult(customer);
        }

        [FunctionName("customers/{id}")]
        public static async Task<IActionResult> TransferPoints([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            TransferPoints points = JsonConvert.DeserializeObject<TransferPoints>(requestBody);
            // this is atomic! Debit and Credit gauranteed Atomic! wooot! take that MongoDB!
            TransferPointsResult transactionResult = await Service.Engine.Execute(log, points);
            return new OkObjectResult(transactionResult);
        }

    }
}
