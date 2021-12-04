#!/usr/local/bin/csharp
#
# (c) Alphons van der Heijden
#
# echo.cs - for testing linux pipe command
#
#
while(true)
{
    var line = Console.ReadLine();
    if(line == null)
        break;
    Console.WriteLine($"*{line}*");
}
Console.WriteLine("-");
