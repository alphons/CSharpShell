//#!/usr/local/bin/csharp
//#
//#
//#
//#
using System;
using System.IO;
using System.Text;
using System.IO.Compression;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Alphons
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var url = "http://repo.tinycorelinux.net/12.x/x86_64/tcz/tags.db.gz";

			var cache = "/tmp/tags.db.gz";

			byte[] gz;

			Console.WriteLine("tce-fetch, loading package list");

			if (File.Exists(cache))
			{
				gz = await File.ReadAllBytesAsync(cache);
			}
			else
			{
				var http = new HttpClient();

				gz = await http.GetByteArrayAsync(url);

				await File.WriteAllBytesAsync(cache, gz);
			}

			using var gzstream = new MemoryStream(gz);

			using var decompressionStream = new GZipStream(gzstream, CompressionMode.Decompress);

			using var ms = new MemoryStream();

			decompressionStream.CopyTo(ms);

			var text = Encoding.UTF8.GetString(ms.ToArray());

			var searchlist = new List<string>();

			var lines = text.Split('\n');

			Console.WriteLine($"Packages loaded: {lines.Length}");

			while (true)
			{
				Console.Write("q=quit l=list s=searh :");

				var k = Console.ReadKey();
				if (k.Key == ConsoleKey.Q)
					break;

				switch (k.Key)
				{
					default:
						break;
					case ConsoleKey.L:
						Console.WriteLine(text);
						break;
					case ConsoleKey.S:
						searchlist.Clear();
						int intI = 1;
						Console.Write("\nSearch for: ");
						var search = Console.ReadLine();
						foreach (var l in lines)
						{
							if (l.ToLower().Contains(search))
							{
								searchlist.Add(l);
								Console.WriteLine($"\t{intI++}. {l}");
							}
						}
						Console.Write($"\nEnter selection (  1 - {intI - 1} ) or (q)uit: ");
						var selection = Console.ReadLine();
						if (int.TryParse(selection, out int intSelection))
						{
							Console.WriteLine($"Todo, info from {searchlist[intSelection - 1]}");
							Console.ReadLine();
						}

						break;
				}
			}

			Console.WriteLine();
		}
	}
}
