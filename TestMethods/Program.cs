// See https://aka.ms/new-console-template for more information

using (var reader = File.OpenText("AFile.txt"))
{
    var fileText = await reader.ReadToEndAsync();
    // Do something with fileText...
    Console.WriteLine(fileText);
}
Console.WriteLine("######################");

string strs = string.Join("\n", File.ReadAllLines("AFile.txt"));
Console.WriteLine(strs);

Console.ReadKey();