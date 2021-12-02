using System.Text;

if (args.Length < 2)
{
    Console.WriteLine($"Usage: {args[0]} <filename>");
    return;
}

var text = await File.ReadAllTextAsync(args[1], Encoding.UTF8);

text = text.Replace("\r\n", "\n");

await File.WriteAllTextAsync(args[1], text, Encoding.ASCII);

