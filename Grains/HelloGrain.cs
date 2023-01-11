using Interfaces;
using Microsoft.Extensions.Logging;
using Orleans;

namespace Grains
{
    public class HelloGrain : Grain, IHello
    {
        private readonly ILogger _logger;
        public string content = "";

        public HelloGrain(ILogger<HelloGrain> logger)
        {
            _logger = logger;
        }

        public override Task OnActivateAsync()
        {
            _logger.LogInformation($"OnActivate is called. ID: {getPrimaryKey()} #################################");
            return base.OnActivateAsync();
        }

        public override Task OnDeactivateAsync()
        {
            _logger.LogInformation($"OnDeactivate is called. ID: {getPrimaryKey()} ---------------------------------");
            return base.OnDeactivateAsync();
        }

        public Task<string> GetContent()
        {
            return Task.FromResult(content);
        }

        public Task<string> SayHello(string greeting)
        {
            var primaryK = this.GetPrimaryKeyLong(out string keyExtension);
            string primaryKey = $"{keyExtension}:{primaryK}";

            // currently when DEACTIVE it destroys the current grain
            this.DeactivateOnIdle();

            _logger.LogInformation($"ID: {primaryKey} SayHello message received: greeting = {greeting}");
            string strs = string.Join("\n", File.ReadAllLines("AFile.txt"));
            content += $"\n Client {primaryKey} said: '{greeting}'. File Content: \n{strs}";
            return Task.FromResult(content);
        }

        private string getPrimaryKey() {
            var primaryKey = this.GetPrimaryKeyLong(out string keyExtension);
            return $"{keyExtension}:{primaryKey}";
        }
    }
}
