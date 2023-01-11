using Interfaces;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Providers;

namespace Grains
{
    [StorageProvider]
    public class HelloGrain : Grain<GreetingArchive>, IHello
    {
        private readonly ILogger _logger;
        public string content = "";

        public HelloGrain(ILogger<HelloGrain> logger)
        {
            _logger = logger;
        }

        public override Task OnActivateAsync()
        {
            // CODE RUNS IN SILO
            _logger.LogInformation($"OnActivate is called. ID: {getPrimaryKey()} #################################");
            return base.OnActivateAsync();
        }

        public override Task OnDeactivateAsync()
        {
            // CODE RUNS IN SILO
            _logger.LogInformation($"OnDeactivate is called. ID: {getPrimaryKey()} ---------------------------------");
            return base.OnDeactivateAsync();
        }

        public Task<string> GetContent()
        {
            return Task.FromResult(content);
        }

        public async Task<string> SayHello(string greeting)
        {

            // when DEACTIVE, the SILO removes from memory
            //this.DeactivateOnIdle();
            State.Greetings.Add(greeting);
            await WriteStateAsync();

            string primaryKey = getPrimaryKey();
            _logger.LogInformation($"ID: {primaryKey} SayHello message received: new greeting = {greeting}");
            string fileContent = string.Join("\n", File.ReadAllLines("AFile.txt"));
            string allGreetings = string.Join("\n", State.Greetings);

            content = $"######\nClient {primaryKey} said: '{allGreetings}'. File Content: \n{fileContent}";
            return $"######\nClient {primaryKey} said: '{greeting}'. File Content: \n{fileContent}";
        }

        private string getPrimaryKey() {
            var primaryKey = this.GetPrimaryKeyLong(out string keyExtension);
            return $"{keyExtension}:{primaryKey}";
        }
    }

    public class GreetingArchive
    {
        public List<string> Greetings { get; private set; } = new List<string>();
    }
}
