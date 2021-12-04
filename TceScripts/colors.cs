#!/usr/local/bin/csharp
#
# (c) Dec 2021 Alphons van der Heijden
#
# colors.cs - Showing terminal colors
#
using System;


// Get an array with the values of ConsoleColor enumeration members.
var colors = (ConsoleColor[])ConsoleColor.GetValues(typeof(ConsoleColor));

// Save the current background and foreground colors.
var currentBackground = Console.BackgroundColor;
var currentForeground = Console.ForegroundColor;

// Display all foreground colors except the one that matches the background.
Console.WriteLine("All the foreground colors except {0}, the background color:",
	 currentBackground);
foreach (var color in colors)
{
	if (color == currentBackground) continue;

	Console.ForegroundColor = color;
	Console.WriteLine("   The foreground color is {0}.", color);
}
Console.WriteLine();
// Restore the foreground color.
Console.ForegroundColor = currentForeground;

// Display each background color except the one that matches the current foreground color.
Console.WriteLine("All the background colors except {0}, the foreground color:",
	currentForeground);
foreach (var color in colors)
{
	if (color == currentForeground) continue;

	Console.BackgroundColor = color;
	Console.WriteLine("   The background color is {0}.", color);
}

// Restore the original console colors.
Console.ResetColor();
Console.WriteLine("\nOriginal colors restored...");
