using System.Text;
using System.Reflection;
using System.Runtime.Loader;
using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;

using Microsoft.Extensions.Configuration;

namespace CSharpShell
{
	internal class Compiler
	{
		private List<MetadataReference> references;

		private string ImplicitUsings;
		public Compiler()
		{
			var Configuration = new ConfigurationBuilder()
			.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
			.Build();

			var dir = Path.GetDirectoryName(typeof(object).Assembly.Location);

			references = new List<MetadataReference>();
			var refs = Configuration.GetSection("References");
			foreach (var r in refs.GetChildren().ToList())
			{
				references.Add(MetadataReference.CreateFromFile(Path.Combine(dir, r.Value)));
			}

			ImplicitUsings = String.Empty;

			var usgs = Configuration.GetSection("Usings");
			foreach (var r in usgs.GetChildren().ToList())
			{
				ImplicitUsings += $"using {r.Value};";
			}
		}

		public object? Execute(string sourceCode, string[] args)
		{
			using var md5 = System.Security.Cryptography.MD5.Create();

			var guid = new Guid(md5.ComputeHash(Encoding.UTF8.GetBytes(sourceCode)));

			var path = @"/var/spool/" + guid + ".bin";

			byte[]? compiledAssembly;

			if (File.Exists(path) && args != null && args.Length>0)
				compiledAssembly = File.ReadAllBytes(path);
			else
				compiledAssembly = CompileSourceCode(sourceCode);

			if (compiledAssembly == null)
				return null;

			if (!File.Exists(path) && args != null && args.Length > 0)
				File.WriteAllBytes(path, compiledAssembly);

			var result = ExecuteAssembly(compiledAssembly, args, out WeakReference assemblyLoadContextWeakRef);

			for (var i = 0; i < 8 && assemblyLoadContextWeakRef.IsAlive; i++)
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
			}

			if(assemblyLoadContextWeakRef.IsAlive)
				Console.WriteLine("Unloading failed!");

			return result;
		}

		private byte[]? CompileSourceCode(string sourceCode)
		{
			using (var peStream = new MemoryStream())
			{
				var result = GenerateCode(ImplicitUsings + sourceCode).Emit(peStream);

				if (!result.Success)
				{
					Console.WriteLine("Compilation done with error.");

					var failures = result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);

					foreach (var diagnostic in failures)
					{
						Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
					}

					return null;
				}

				peStream.Seek(0, SeekOrigin.Begin);

				return peStream.ToArray();
			}
		}
		private CSharpCompilation GenerateCode(string sourceCode)
		{
			var codeString = SourceText.From(sourceCode);

			var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp10);

			var parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(codeString, options);

			return CSharpCompilation.Create(null,
				new[] { parsedSyntaxTree },
				references: references,
				options: new CSharpCompilationOptions(OutputKind.ConsoleApplication,
				optimizationLevel: OptimizationLevel.Release,
				assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));
		}
		

		[MethodImpl(MethodImplOptions.NoInlining)]
		private object? ExecuteAssembly(byte[] compiledAssembly, string[] args, out WeakReference weakReference)
		{
			using (var asm = new MemoryStream(compiledAssembly))
			{
				var assemblyLoadContext = new SimpleUnloadableAssemblyLoadContext();

				var assembly = assemblyLoadContext.LoadFromStream(asm);

				var entry = assembly.EntryPoint;

				object? result = null;

				if (entry != null)
				{
					if (entry.GetParameters().Length > 0)
						result = entry.Invoke(null, new object[] { args });
					else
						result = entry.Invoke(null, null);
				}
				assemblyLoadContext.Unload();

				weakReference = new WeakReference(assemblyLoadContext);

				return result;
			}
		}

		private class SimpleUnloadableAssemblyLoadContext : AssemblyLoadContext
		{
			public SimpleUnloadableAssemblyLoadContext()
				: base(true)
			{
			}

			protected override Assembly Load(AssemblyName assemblyName)
			{
				return null;
			}
		}

	}
}
