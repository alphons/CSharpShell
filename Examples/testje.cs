#!/usr/local/bin/csharp
#
# (C) Alphons
#
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

