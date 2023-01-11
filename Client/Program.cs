﻿using Interfaces;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using System.Net;

namespace Client
{
    internal class Program
    {
        public static async Task<int> Main(string[] args)
        {
            try
            {
                using (var client = await ConnectClientAsync())
                {
                    Console.WriteLine($"Client IsInitialized: {client.IsInitialized}");

                    await DoClientWorkAsync(client);
                    Thread.Sleep(1000 * 2);
                    await DoClientVerificationAsync(client);
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

        static async Task<IClusterClient> ConnectClientAsync()
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

            await client.Connect();
            Console.WriteLine("Client successfully connected to silo host!");

            return client;
        }

        static async Task DoClientWorkAsync(IClusterClient client)
        {
            var friend = client.GetGrain<IHello>(0, "key");
            var response = await friend.SayHello("Good morning HelloGrain!");

            Console.WriteLine($"\n{response}\n");
            Globals.grainRef = friend;
        }


        static async Task DoClientVerificationAsync(IClusterClient client)
        {
            var friend = client.GetGrain<IHello>(0, "key");
            var response = await friend.SayHello("Good evening HelloGrain!");

            Console.WriteLine($"\n friend2: {response}\n");
            var friend1Response = await Globals.grainRef.GetContent();
            Console.WriteLine($"friend1: {friend1Response}");
            Console.WriteLine($"equasl? {friend == Globals.grainRef}");
        }

    }
}
