# CSharpShell
## _running unix shell scripts coded in C# .NET Core code_

```sh
#!/usr/local/bin/csharp

Console.WriteLine("Hello, World!");
```

Getting around the UTF8 / Windows to Unix thingy, doing it the Async way

```
#!/usr/local/bin/csharp

var text = await File.ReadAllTextAsync(args[1], Encoding.UTF8);

text = text.Replace("\r\n", "\n");

await File.WriteAllTextAsync(args[1], text, Encoding.ASCII);
```


## First time compiles, assembly is stored in user defined cache directory

```
#!/usr/local/bin/csharp
#
# testing - script to measure executing speed

using System;
using System.Diagnostics;

var sw = Stopwatch.StartNew();

Console.WriteLine("Started");

for(int n=1;n<9;n++)
{
	double a = 0;
	var end = Math.Pow (10, n);
	for(int intI=0;intI<=end;intI++)
	{
		a += intI;
	}
	Console.WriteLine($"{sw.ElapsedMilliseconds}mS {end} {a}");
}
```

```
$ time ./testing
Started
8mS 10 55
8mS 100 5050
8mS 1000 500500
8mS 10000 50005000
9mS 100000 5000050000
12mS 1000000 500000500000
38mS 10000000 50000005000000
299mS 100000000 5000000050000000
real    0m 2.57s
user    0m 2.50s
sys     0m 0.06s

$ time ./testing
Started
8mS 10 55
8mS 100 5050
8mS 1000 500500
8mS 10000 50005000
9mS 100000 5000050000
11mS 1000000 500000500000
37mS 10000000 50000005000000
298mS 100000000 5000000050000000
real    0m 0.39s
user    0m 0.38s
sys     0m 0.00s
```
