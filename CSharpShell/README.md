# CSharpShell

Make symbolic link in the /usr/local/bin directory to the CSharpShell directory.
In this example this shell dir is /home/tc/CSharpShell

```
ln -s /home/tc/CSharpShell/csharp /usr/local/bin/csharp
```

The wrapper script csharp is included and calls the Shell interpreter

```
#!/bin/sh
#
# csharp 1.0 wrapper
#
dotnet /home/tc/CSharpShell/CSharpShell.dll $1 $2 $3 $4 $5 $6 $7 $8 $9
```

Example script listing arguments

```c#
#!/usr/local/bin/csharp
#
#

for(int i=0;i<args.Length;i++)
	Console.WriteLine($"{i} {args[i]}");
```

## All Scripts must have \n line endings, the windows \r\n will NOT work


