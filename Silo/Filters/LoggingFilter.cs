using Orleans;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Reflection;

namespace Silo.Filters
{
    public class LoggingFilter : IIncomingGrainCallFilter
    {
        private readonly GrainInfo _grainInfo;
        private readonly ILogger<LoggingFilter> _logger;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public LoggingFilter(
            GrainInfo grainInfo,
            ILogger<LoggingFilter> logger,
            JsonSerializerSettings jsonSerializerSettings)
        {
            _grainInfo= grainInfo;
            _logger = logger;
            _jsonSerializerSettings = jsonSerializerSettings;
        }

        public async Task Invoke(IIncomingGrainCallContext context)
        {
            try
            {
                LogInfo(context, request: true);
                await context.Invoke();
                LogInfo(context, request: false);
            }
            catch (Exception ex)
            {
                var arguments = JsonConvert.SerializeObject(context.Arguments, _jsonSerializerSettings);
                var result = JsonConvert.SerializeObject(context.Result, _jsonSerializerSettings);
                _logger.LogError($"LOGGINFILTER {context.Grain.GetType()}.{context.InterfaceMethod.Name}:  threw an exception {nameof(ex)} request", ex);
                // throw;
            }
        }

        private void LogInfo(IIncomingGrainCallContext context, bool request)
        {
            string methodName = context.InterfaceMethod.Name;
            if (ShouldLog(methodName))
            {
                string data;
                if (request)
                {
                    var arguments = JsonConvert.SerializeObject(context.Arguments, _jsonSerializerSettings);
                    data = $"arguments: {arguments}";
                }
                else
                {
                    var result = JsonConvert.SerializeObject(context.Result, _jsonSerializerSettings);
                    data = $"result: {result}";
                }

                _logger.LogInformation($"LOGGINFILTER {context.Grain.GetType()}.{methodName}: {data} request");
            }
        }

        private bool ShouldLog(string methodName)
        {
            return _grainInfo.Methods.Contains(methodName);
        }
    }
}
