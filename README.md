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
