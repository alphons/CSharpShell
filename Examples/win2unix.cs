#!/usr/local/bin/csharp
#
# win2unix
#
using System.Text;

if (args.Length < 2)
{
	Console.WriteLine($"Arguments count:{args.Length}");
	Console.WriteLine($"Usage: {args[0]} <filename>");
	return;
}

var fileName = args[1];

if (!File.Exists(fileName))
{
	Console.WriteLine($"Filename does not exist: {fileName}");
	return;
}

var text = await File.ReadAllTextAsync(fileName, Encoding.UTF8);

await File.WriteAllTextAsync(fileName, text.Replace("\r\n", "\n"), Encoding.ASCII);
