using Grains;
using Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Silo.Context;
using Silo.Filters;
using System.Net;

namespace Silo
{
    internal class Program
    {
        static readonly ManualResetEvent _siloStopped = new ManualResetEvent(false);
        static bool siloStopping;
        static readonly object syncLock = new object();
        static IHost host;

        public static async Task<int> Main(string[] args)
        {
            try
            {
                SetupApplicationShutdown();

                host = await StartSiloAsync();
                Console.WriteLine("\nSILO STARTED\nCTRL+C to TERMINATE...\n\n");

                _siloStopped.WaitOne();
                Console.WriteLine("SILO WAS STOPPED");
                Console.ReadKey();
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
                    c.Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = "dev";
                        options.ServiceId = "OrleansBasics";
                    })
                    // just one cluster
                    //.UseLocalhostClustering()
                    // use clustering
                    .UseAdoNetClustering(options => {
                        options.Invariant = orleansConfig.Invariant;
                        options.ConnectionString = orleansConfig.ConnectionString;
                    })
                    .Configure<EndpointOptions>(options =>
                    {
                        options.SiloPort = orleansConfig.SiloPort;
                        options.GatewayPort = orleansConfig.GatewayPort;
                        options.AdvertisedIPAddress = IPAddress.Loopback;
                    })

                    // add DI to inject in LoggingFilter
                    .ConfigureServices(services => {
                        services.AddSingleton<IOrleansRequestContext, OrleansRequestContext>();
                        // need require.d "s" without it is not registered
                        services.AddSingleton(s => CreateGrainMethodsList());
                        services.AddSingleton(s => new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            Formatting = Formatting.None,
                            TypeNameHandling = TypeNameHandling.None,
                            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                        });
                    })

                    .AddStateStorageBasedLogConsistencyProvider("StateStorage")

                    // capture logging of Grain in server side
                    .AddIncomingGrainCallFilter<LoggingFilter>()

                    .AddAdoNetGrainStorageAsDefault(options =>
                    {
                        options.Invariant = orleansConfig.Invariant;
                        options.ConnectionString = orleansConfig.ConnectionString;
                        options.UseJsonFormat = true;
                    })
                    .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(HelloGrain).Assembly).WithReferences())
                    .ConfigureLogging(logging => logging.AddConsole());

                    if (orleansConfig.UseDashboard)
                    {
                        c.UseDashboard();
                    }
                });

            var host = builder.Build();
            await host.StartAsync();

            return host;
        }

        static void SetupApplicationShutdown()
        {
            Console.CancelKeyPress += Console_CancelKeyPress;
        }

        private static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            lock (syncLock)
            {
                if (!siloStopping) {
                    siloStopping = true;
                    Task.Run(StopSilo).Ignore();
                }
            }
        }

        static async Task StopSilo()
        {
            await host.StopAsync();
            _siloStopped.Set();
        }

        private static GrainInfo CreateGrainMethodsList()
        {
            IEnumerable<string> methodNames = typeof(IHello).Assembly.GetTypes()
                                    .Where(type => type.IsInterface)
                                    .SelectMany(type => type.GetMethods()
                                                .Select(methodInfo => methodInfo.Name)
                                    ).Distinct();

            return new GrainInfo() { Methods = methodNames.ToList() };
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
}