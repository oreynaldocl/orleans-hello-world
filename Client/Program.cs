﻿using Interfaces;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Runtime.Messaging;
using Polly;
using System.Net;

namespace Client
{
    internal class Program
    {
        public static async Task<int> Main(string[] args)
        {
            try
            {
                using (var client = ConnectClientAsync())
                {
                    Console.WriteLine($"Client IsInitialized: {client.IsInitialized}");

                    RequestContext.Set("traceId", Guid.NewGuid());
                    await CallGreetingsGrain(client, GetGrainKey());
                    await DoClientWorkAsync(client, GetGrainKey());

                    Thread.Sleep(1000 * 2);

                    RequestContext.Set("traceId", Guid.NewGuid());
                    await DoClientVerificationAsync(client, GetGrainKey());
                    Console.ReadKey();
                }

                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine($"\nException while trying to run client: {e.Message}");
                Console.WriteLine("Make sure the silo the client is trying to connect to is running.");
                Console.WriteLine("\nPress any key to exit.");
                Console.ReadKey();
                return -1;
            }
        }

        private static int GetGrainKey()
        {
            Console.Write("Give Grain Key=");
            string? keyNum = Console.ReadLine();
            if (!int.TryParse(keyNum, out int keyNumber))
            {
                Console.WriteLine("Using default Key: 0");
                keyNumber = 0;
            }
            return keyNumber;
        }

        static IClusterClient ConnectClientAsync()
        {
            return Policy<IClusterClient>
                .Handle<SiloUnavailableException>()
                .Or<ConnectionFailedException>()
                .WaitAndRetry(new[] {
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(4),
                    TimeSpan.FromSeconds(8),
                    TimeSpan.FromSeconds(16),
                }).Execute(() =>
                {
                    var client = new ClientBuilder()
                        .UseLocalhostClustering()
                        .Configure<ClusterOptions>(options =>
                        {
                            options.ClusterId = "dev";
                            options.ServiceId = "OrleansBasics";
                        })
                        .ConfigureLogging(logging => logging.AddConsole())
                        .Build();

                    client.Connect().GetAwaiter().GetResult();
                    Console.WriteLine("Client successfully connected to silo host!");

                    return client;
                });
        }

        static async Task CallGreetingsGrain(IClusterClient client, int grainKey)
        {
            var grain = client.GetGrain<IGreetingsGrain>(grainKey);
            await grain.SendGreetings("Hello");

            var grain1 = client.GetGrain<IGreetingsGrain>(grainKey);
            await grain1.SendGreetings("Good afternoon");

            var grain2 = client.GetGrain<IGreetingsGrain>(grainKey);
            await grain2.SendGreetings("Good bye");
        }

        static async Task DoClientWorkAsync(IClusterClient client, int keyNum)
        {
            var grain = client.GetGrain<IHello>(keyNum, "key");
            var response = await grain.SayHello("Good morning HelloGrain!");
            Console.WriteLine($"\n{response}\n");
            Globals.grainRef = grain;
        }


        static async Task DoClientVerificationAsync(IClusterClient client, int keyNum)
        {
            var friend = client.GetGrain<IHello>(keyNum, "key");
            var response = await friend.SayHello("Good evening HelloGrain!");

            Console.WriteLine($"\n friend2: {response}\n");
            var friend1Response = await Globals.grainRef.GetContent();
            Console.WriteLine($"friend1: {friend1Response}");
        }

    }
}
