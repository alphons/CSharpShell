using System.Text;
using System.Diagnostics;

// dotnet publish -r linux-x64 -p:PublishSingleFile=true --self-contained false

var compiler = new CSharpShell.Compiler();

object? result;

if (Debugger.IsAttached)
{
	for (int intI = 0; intI < 10; intI++)
	{
		result = compiler.Execute("Console.WriteLine();", new string[0]);
	}
	return;
}

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
	sb.AppendLine(line);
}
var cs = sb.ToString();

try
{
	result = compiler.Execute(cs, args);
}
catch (Exception ex)
{
	result = $"{ex.Message}{Environment.NewLine}{ex.StackTrace}";
}

if (result != null)
	Console.WriteLine(result + "");
