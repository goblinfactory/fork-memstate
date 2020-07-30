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
        [FunctionName("customers-get-all")]
        public static async Task<IActionResult> CustomersGetAll([HttpTrigger(AuthorizationLevel.Function, "GET", Route = "customers")] HttpRequest req, ILogger log)
        {
            var customers = await Service.Engine.Execute(log, new GetCustomers());
            return new JsonResult(customers);
        }

        [FunctionName("customers-top-10")]
        public static async Task<IActionResult> CustomersTop10([HttpTrigger(AuthorizationLevel.Function, "GET", Route = "customers/top-10")] HttpRequest req, ILogger log)
        {
            var customers = await Service.Engine.Execute(log, new Top10Customers());
            return new JsonResult(customers);
        }

        [FunctionName("customer-earn-points")]
        public static async Task<IActionResult> CustomerEarnPoints([HttpTrigger(AuthorizationLevel.Function, "POST", Route = "customers/{id}/points-earned")] HttpRequest req, ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            EarnPoints earnPointsCmd = JsonConvert.DeserializeObject<EarnPoints>(requestBody);
            Customer customer = await Service.Engine.Execute(log, earnPointsCmd);
            return new OkObjectResult(customer);
        }

        [FunctionName("customer-init")]
        public static async Task<IActionResult> Init([HttpTrigger(AuthorizationLevel.Function, "PUT", Route = "customers/{id}")] HttpRequest req, ILogger log, int id)
        {
            return await Service.Engine.ExecuteCommand<InitCustomer, LoyaltyDB, Customer>(req, log, cmd => cmd.CustomerId.EnsureIDMatches(id));
        }

        [FunctionName("customer-spend-points")]
        public static async Task<IActionResult> SpendPoints([HttpTrigger(AuthorizationLevel.Function, "POST", Route = "customers/{id}/points-spent")] 
            HttpRequest req, ILogger log, int id)
        {
            return await Service.Engine.ExecuteCommand<TransferPoints, LoyaltyDB, TransferPointsResult>(req, log, cmd =>cmd.SenderId.EnsureIDMatches(id));
        }


        //this is atomic!Debit and Credit gauranteed Atomic!wooot! take that MongoDB!
        [FunctionName("customer-transfer-points")] 
        public static async Task<IActionResult> TransferPoints([HttpTrigger(AuthorizationLevel.Function, "POST", Route = "customers/{id}/points-tranferred2")] 
        HttpRequest req, ILogger log, int id)
        {
            return await Service.Engine.ExecuteCommand<TransferPoints, LoyaltyDB, TransferPointsResult>(req, log, cmd => cmd.SenderId.EnsureIDMatches(id, log));
        }

    }
}

// learning references 
// -------------------
// Azure functions REST and function routes
// https://docs.microsoft.com/en-us/learn/modules/build-api-azure-functions/5-rest-function-routes

// Azure Table Storage disallowed characters
// todo: add to Goblinfactory.Azure.TableStorage
// https://microsoft.github.io/AzureTipsAndTricks/blog/tip87.html

// azure function filters
// https://github.com/Azure/azure-webjobs-sdk/wiki/Function-Filters
// be careful of memory leaks! a million requests can cause a serious leak.

// aspnetcore 3.1 filters
// https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters?view=aspnetcore-3.1

// exception filters
// https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters?view=aspnetcore-3.1#exception-filters