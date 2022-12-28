using System.Text;
using System.Diagnostics;

//nuget Microsoft.CodeAnalysis.CSharp
//nuget Microsoft.Extensions.Configuration.Json

var compiler = new CSharpShell.Compiler();

object? result;

if (args.Length == 0 || Debugger.IsAttached == true)
{
	Console.WriteLine("CSharpShell 1.0 (exit = quit)");
	// Interactive
	while (true)
	{
		var line = Console.ReadLine();
		if (line == null)
			break;

		if (line == "exit" || line == "quit")
			break;

		try
		{
			var sw = Stopwatch.StartNew();
			result = await compiler.ExecuteAsync(line, args);
			Console.WriteLine(sw.ElapsedMilliseconds + "mS");
		}
		catch (Exception ex)
		{
			result = $"{ex.Message}{Environment.NewLine}{ex.StackTrace}";
		}

		if (result != null)
			Console.WriteLine(result + "");
	}
	return;
}


var path = Directory.GetCurrentDirectory() + args[0].Trim('.');

var sb = new StringBuilder();

using var sr = new StreamReader(path);

while (!sr.EndOfStream)
{
	var line = await sr.ReadLineAsync();
	if (line == null)
		break;
	if (line.StartsWith('#'))
		continue;
	sb.AppendLine(line);
}
var cs = sb.ToString();

try
{
	result = await compiler.ExecuteAsync(cs, args);
}
catch (Exception ex)
{
	result = $"{ex.Message}{Environment.NewLine}{ex.StackTrace}";
}

if (result != null)
	Console.WriteLine(result + "");
