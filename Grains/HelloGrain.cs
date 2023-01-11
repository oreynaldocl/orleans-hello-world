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

        public Task<string> GetContent()
        {
            return Task.FromResult(content);
        }

        public Task<string> SayHello(string greeting)
        {
            _logger.LogInformation("SayHello message received: greeting = {Greeting}", greeting);
            string strs = string.Join("\n", File.ReadAllLines("AFile.txt"));
            content += $"\n Client said: '{greeting}'. File Content: \n{strs}";
            return Task.FromResult(content);
        }
    }
}
