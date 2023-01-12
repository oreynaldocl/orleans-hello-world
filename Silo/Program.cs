using Grains;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using System.Net;

namespace Silo
{
    internal class Program
    {
        public static async Task<int> Main(string[] args)
        {
            try
            {
                var host = await StartSiloAsync();
                Console.WriteLine("\nSILO STARTED \n Press Enter to terminate...\n\n");
                Console.ReadLine();

                await host.StopAsync();
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return -1;
            }
        }

        private static async Task<IHost> StartSiloAsync()
        {
            var config = LoadConfig();
            var orleansConfig = GetOrleansConfig(config);

            var builder = new HostBuilder()
                .UseOrleans(c =>
                {
                    c.UseLocalhostClustering()
                    .Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = "dev";
                        options.ServiceId = "OrleansBasics";
                    })
                    .Configure<EndpointOptions>(options =>
                    {
                        options.SiloPort = 11111;
                        options.GatewayPort = 30000;
                        options.AdvertisedIPAddress = IPAddress.Loopback;
                    })
                    .UseDashboard()
                    .AddAdoNetGrainStorageAsDefault(options =>
                    {
                        options.Invariant = orleansConfig.Invariant;
                        options.ConnectionString = orleansConfig.ConnectionString;
                        options.UseJsonFormat = true;
                    })
                    .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(HelloGrain).Assembly).WithReferences())
                    .ConfigureLogging(logging => logging.AddConsole());
                });

            var host = builder.Build();
            await host.StartAsync();

            return host;
        }

        private static IConfigurationRoot LoadConfig()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("appsettings.json");
            var config = configurationBuilder.Build();
            return config;
        }

        private static OrleansConfig GetOrleansConfig(IConfigurationRoot config)
        {
            var orleansConfig = new OrleansConfig();
            var section = config.GetSection("OrleansConfiguration");
            section.Bind(orleansConfig);
            return orleansConfig;
        }
    }

    public class OrleansConfig
    {
        public string Invariant { get; set; }
        public string ConnectionString { get; set; }
    }
}