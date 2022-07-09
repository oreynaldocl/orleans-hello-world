using Grains.Interfaces;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;

try
{
    using (var client = await ConnectClientAsync())
    {
        await DoClientWorkAsync(client);
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
    return 1;
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
    Console.WriteLine("Client successfully connected to silo host! \n");

    return client;
}

static async Task DoClientWorkAsync(IClusterClient client)
{
    var friend = client.GetGrain<IHello>(0);
    var response = await friend.SayHello("Good morning HelloGrain!");

    Console.WriteLine($"\n\n{response}\n\n");
}