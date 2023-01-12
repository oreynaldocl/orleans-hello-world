using DbUp;
using System.Reflection;

namespace DbMigration
{
    internal class Program
    {
        static int Main(string[] args)
        {
            var connectionString =
                args.FirstOrDefault()
                ?? "Server=127.0.0.1;Database=OrleansHelloWorld;Uid=root;Pwd=Control*123;";

            var upgrader =
                DeployChanges.To
                    .SqlDatabase(connectionString)
                    .WithExecutionTimeout(TimeSpan.FromMinutes(10))
                    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                    .WithExecutionTimeout(TimeSpan.FromMinutes(10))
                    .LogToConsole()
                    .Build();

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(result.Error);
                Console.ResetColor();
#if DEBUG
                Console.ReadLine();
#endif
                return -1;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Success!");
            Console.ResetColor();
            return 0;
        }
    }
}