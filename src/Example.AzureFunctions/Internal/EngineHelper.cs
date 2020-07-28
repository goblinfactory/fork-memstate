using Memstate;
using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.IO;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;

namespace Memstate.Examples.AzureFunctions
{
    public static class EngineHelper
    {
        public static Task<TResult> Execute<TModel, TResult>(this Engine<TModel> engine, Microsoft.Extensions.Logging.ILogger log, Query<TModel, TResult> query) where TModel : class
        {
            log.LogInformation(query.ToString());
            return engine.Execute(query);
        }

        public static Task<TResult> Execute<TModel, TResult>(this Engine<TModel> engine, Microsoft.Extensions.Logging.ILogger log, Command<TModel, TResult> command) where TModel : class
        {
            log.LogInformation(command.ToString());
            return engine.Execute(command);
        }

        public static async Task<IActionResult> ExecuteCommand<TCommand, TModel, TResult>(this Engine<TModel> engine, HttpRequest req, Microsoft.Extensions.Logging.ILogger log) 
            where TModel : class 
            where TCommand : Command<TModel, TResult> 
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            TCommand cmd = JsonConvert.DeserializeObject<TCommand>(requestBody);
            TResult result = await engine.Execute(cmd);
            return new OkObjectResult(result);
        }

        public static void EnsureIDMatches(this string idField, string id, Microsoft.Extensions.Logging.ILogger log)
        {
            if (string.IsNullOrWhiteSpace(idField)) throw new ArgumentNullException(nameof(idField),"id cannot be empty");
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id), "id cannot be empty");
            if(!String.Equals(idField, id))
            {
                var msg = $"id's do not match [{idField}] !== [{id}]";
                log.LogError(msg);
                throw new InvalidOperationException(msg);
            }
        }

        public static void EnsureIDMatches(this int idField, int id, Microsoft.Extensions.Logging.ILogger log)
        {
            if (idField==0) throw new ArgumentNullException(nameof(idField), "id cannot be zero.");
            if (id == 0) throw new ArgumentNullException(nameof(id), "id cannot be zero.");
            if (idField != id)
            {
                var msg = $"id's do not match [{idField}] !== [{id}]";
                log.LogError(msg);
                throw new InvalidOperationException(msg);
            }
        }

    }
}
