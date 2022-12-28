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
# csharp 1.0
#
/home/tc/CSharpShell/CSharpShell $1
```

Example script listing arguments

```c#
#!/usr/local/bin/csharp
#
#

for(int i=0;i<args.Length;i++)
	Console.WriteLine($"{i} {args[i]}");
```