#!/usr/local/bin/csharp
#
# (c) Alphons van der Heijden
# 
# tce-ab.cs - Example script TinyCore
#
#

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
		const string url = "http://repo.tinycorelinux.net/12.x/x86_64/tcz/";

		static async Task Main(string[] args)
		{
			var tags = "tags.db.gz";
			var cache = "/tmp/" + tags;

			byte[] gz;

			Console.WriteLine("tce-ab - Tiny Core Extension: Application Browser");

			if (File.Exists(cache))
			{
				gz = await File.ReadAllBytesAsync(cache);
			}
			else
			{
				var http = new HttpClient();

				gz = await http.GetByteArrayAsync(url + tags);

				await File.WriteAllBytesAsync(cache, gz);
			}

			using var gzstream = new MemoryStream(gz);

			using var decompressionStream = new GZipStream(gzstream, CompressionMode.Decompress);

			using var ms = new MemoryStream();

			await decompressionStream.CopyToAsync(ms);

			var text = Encoding.Default.GetString(ms.ToArray());

			var lines = text.Split('\n').ToList();

			Console.WriteLine($"Packages loaded: {lines.Count}");

			var name = string.Empty;

			var msg = await Info(null);

			while (true)
			{
				Console.Write(msg);

				var k = Console.ReadKey();
				if (k.Key == ConsoleKey.Q)
					break;

				switch (k.Key)
				{
					default:
						break;
					case ConsoleKey.A:
						await Info(name);
						break;
					case ConsoleKey.L:
						Console.WriteLine(text);
						break;
					case ConsoleKey.S:
						int intI = 1;
						Console.Clear();
						Console.Write("Enter starting chars or desired extension, e.g. abi: ");
						var search = Console.ReadLine();
						if (search == null)
							continue;

						Console.Clear();
						Console.WriteLine("\ntce - Tiny Core Extension browser\n");

						var searchlist = lines
							.Where( x => x.Contains(search, StringComparison.OrdinalIgnoreCase))
							.ToList();

						foreach (var l in searchlist)
							Console.WriteLine($"\t{intI++}. {l}");

						Console.Write($"\nEnter selection (  1 - {intI - 1} ) or (q)uit: ");
						var selection = Console.ReadLine();
						if (selection?.ToLower() == "q")
							continue;

						if (int.TryParse(selection, out int intSelection))
						{
							var extension = searchlist[intSelection - 1];
							name = extension.Split('\t')[0];
							msg = await Info(name);
						}

						break;
				}
			}

			Console.WriteLine();
		}

		private async static Task<string> Info(string? name)
		{
			if (string.IsNullOrWhiteSpace(name))
				return "S)earch P)rovides K)eywords or Q)uit: ";
			using var http = new HttpClient();
			var info = await http.GetStringAsync(url + name + ".info");
			Console.Clear();
			Console.WriteLine(info);
			return "A)bout I)nstall O)nDemand D)epends T)ree F)iles siZ)e S)earch P)provides K)eywords or Q)uit: ";

		}
	}
}
