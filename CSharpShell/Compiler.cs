using System.Text;
using System.Reflection;
using System.Runtime.Loader;
using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;


namespace CSharpShell
{
	internal class Compiler
	{
		private readonly List<MetadataReference>? references;

		private readonly string? CacheDir;

		private readonly string? ImplicitUsings;

		[UnconditionalSuppressMessage("SingleFile", "IL3000:Avoid accessing Assemblfiley file path when publishing as a single file", Justification = "<Pending>")]
		public Compiler()
		{
			try
			{
				var Configuration = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.Build();

				this.references = GetMetadataReferences(Configuration);

				this.CacheDir = GetCacheDir(Configuration);

				this.ImplicitUsings = GetImplicitUsings(Configuration);
			}
			catch (Exception eee)
			{
				Console.Error.WriteLine($"Compiler error: {eee.Message}");
			}
		}

		private static string? GetCacheDir(IConfigurationRoot Configuration)
		{
			return Configuration["CacheDir"];
		}

		private static string? GetImplicitUsings(IConfigurationRoot Configuration)
		{
			var ImplicitUsings = string.Empty;

			var usgs = Configuration.GetSection("Usings");
			foreach (var r in usgs.GetChildren().ToList())
			{
				ImplicitUsings += $"using {r.Value};";
			}

			return ImplicitUsings;
		}

		private static List<MetadataReference>? GetMetadataReferences(IConfigurationRoot Configuration)
		{
			//var netCoreVer = System.Environment.Version;
			//var runtimeVer = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
			//var dirRuntime = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();
			// "C:\Program Files\dotnet\shared\Microsoft.NETCore.App\6.0.0\"

			var references = new List<MetadataReference>();

			var RefNames = new string[] { "NETCore", "AspNetCore" };
			var RefTypes = new string[] { "System", "Microsoft.AspNetCore.Metadata" }; // Metadata is small 

			for (int intI = 0; intI < RefNames.Length; intI++)
			{
				var refs = Configuration.GetSection(RefNames[intI]);
				if (refs == null)
					continue;

				string? dir = null;

				try
				{
					var assembly = AppDomain.CurrentDomain.Load(RefTypes[intI]);
#pragma warning disable IL3000 // Avoid accessing Assembly file path when publishing as a single file
					dir = Path.GetDirectoryName(assembly.Location);
#pragma warning restore IL3000 // Avoid accessing Assembly file path when publishing as a single file
				}
				catch
				{
				}
				foreach (var r in refs.GetChildren().ToList())
				{
					if (string.IsNullOrWhiteSpace(dir))
					{
						Console.Error.WriteLine($"Framework: {RefTypes[intI]} not found, missing SDK?");
						break;
					}
					if (r.Value == null)
						continue;
					var path = Path.Combine(dir, r.Value);
					if (File.Exists(path))
						references.Add(MetadataReference.CreateFromFile(path));
					else
						Console.Error.WriteLine($"appsettings.json value not found: {r.Value} section:{RefNames[intI]} directory:{dir}");
				}
			}

			return references;
		}

		public async Task<object?> ExecuteAsync(string sourceCode, string[] args)
		{
			var guid = new Guid(System.Security.Cryptography.MD5.HashData(Encoding.UTF8.GetBytes(sourceCode)));

			var path = $"{this.CacheDir}/{guid}.dll";

			byte[]? compiledAssembly;

			if (File.Exists(path) && args != null && args.Length>0)
				compiledAssembly = await File.ReadAllBytesAsync(path);
			else
				compiledAssembly = CompileSourceCode(sourceCode);

			if (compiledAssembly == null)
				return null;

			if (!File.Exists(path) && args != null && args.Length > 0)
				await File.WriteAllBytesAsync(path, compiledAssembly);

			var result = ExecuteAssembly(compiledAssembly, args, out WeakReference assemblyLoadContextWeakRef);

			for (var i = 0; i < 8 && assemblyLoadContextWeakRef.IsAlive; i++)
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
			}

			if(assemblyLoadContextWeakRef.IsAlive)
				Console.Error.WriteLine("Unloading failed!");

			return result;
		}

		private byte[]? CompileSourceCode(string sourceCode)
		{
			using var peStream = new MemoryStream();

			var csharpCompilation = GenerateCode(ImplicitUsings + sourceCode);

			var sw = Stopwatch.StartNew();
			var result = csharpCompilation.Emit(peStream);

			if (Debugger.IsAttached)
				Console.WriteLine("Emit:" + sw.ElapsedMilliseconds + "mS");

			if (!result.Success)
			{
				var failures = result.Diagnostics
					.Where(diagnostic => diagnostic.IsWarningAsError ||
					diagnostic.Severity == DiagnosticSeverity.Error);

				foreach (var diagnostic in failures)
				{
					Console.Error.WriteLine($"{diagnostic.Id}: {diagnostic.GetMessage()}");
				}

				return null;
			}

			peStream.Seek(0, SeekOrigin.Begin);

			return peStream.ToArray();
		}
		private CSharpCompilation GenerateCode(string sourceCode)
		{
			var codeString = SourceText.From(sourceCode);

			var parsedSyntaxTreeOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp11);

			var parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(codeString, parsedSyntaxTreeOptions);

			var cSharpCompilationOptions = new CSharpCompilationOptions(OutputKind.ConsoleApplication,
				optimizationLevel: OptimizationLevel.Release,
				assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default);

			return CSharpCompilation.Create(null,new[] { parsedSyntaxTree }, references: references, options: cSharpCompilationOptions);
		}



		[MethodImpl(MethodImplOptions.NoInlining)]
		private static object? ExecuteAssembly(byte[] compiledAssembly, string[]? args, out WeakReference weakReference)
		{
			using var asm = new MemoryStream(compiledAssembly);

			var assemblyLoadContext = new SimpleUnloadableAssemblyLoadContext();

			var assembly = assemblyLoadContext.LoadFromStream(asm);

			var entry = assembly.EntryPoint;

			object? result = null;

			if (entry != null)
			{
				if (entry.GetParameters().Length > 0)
					result = entry.Invoke(null, args == null ? null : new object[] { args });
				else
					result = entry.Invoke(null, null);
			}
			assemblyLoadContext.Unload();

			weakReference = new WeakReference(assemblyLoadContext);

			return result;
		}

		private class SimpleUnloadableAssemblyLoadContext : AssemblyLoadContext
		{
			public SimpleUnloadableAssemblyLoadContext()
				: base(true)
			{
			}

			protected override Assembly Load(AssemblyName assemblyName)
			{
#pragma warning disable CS8603 // Possible null reference return.
				return null;
#pragma warning restore CS8603 // Possible null reference return.
			}
		}

	}
}
