using System.Text;
using System.Diagnostics;

if (args.Length == 0 && Debugger.IsAttached == false)
{
	Console.WriteLine("No arguments");
	return;
}

var compiler = new CSharpShell.Compiler();

object? result;

if (Debugger.IsAttached)
{
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
			result = compiler.Execute(line, args);
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
	var line = sr.ReadLine();
	if (line == null)
		break;
	if (line.StartsWith('#'))
		continue;
	sb.Append(line);
}
var cs = sb.ToString();

try
{
	result = compiler.Execute(cs, new string[0]);
}
catch (Exception ex)
{
	result = $"{ex.Message}{Environment.NewLine}{ex.StackTrace}";
}

if (result != null)
	Console.WriteLine(result + "");
