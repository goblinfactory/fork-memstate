using Memstate;
using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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

    }
}
