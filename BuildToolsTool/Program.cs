using System.Diagnostics;
using Newtonsoft.Json;
using Silk.NET.BuildTools;
using Silk.NET.BuildTools.Common;

namespace BuildToolsTool;

public static class Program {
	private static Config ExampleJsonFile = new Config {
		Tasks = new[] {
			new BindTask {
				BakeryOpts = new BakeryOptions {
					Include = new[] { "webgpu" }
				},
				CacheFolder = "/build/cache",
				CacheKey    = "webgpu",
				ClangOpts = new ClangTaskOptions {
					ClangArgs = new[] {
						"--language=c++",
						"--std=c++17",
						"-m64",
						"-Wno-expansion-to-defined",
						"-Wno-ignored-attributes",
						"-Wno-ignored-pragma-intrinsic",
						"-Wno-nonportable-include-path",
						"-Wno-pragma-pack",
						"-I$windowsSdkIncludes",
						"-Ipath/to/library/include/"
					},
					ClassMappings = new Dictionary<string, string> {
						{ "webgpu.h", "[Core]WebGPU" }
					}
				},
				Controls = new[] {
					"convert-windows-only",
					"no-obsolete-enum"
				},
				FunctionPrefix      = "wgpu",
				ExtensionsNamespace = "Silk.NET.WebGPU.Extensions",
				Namespace           = "Silk.NET.WebGPU",
				Mode                = ConverterMode.Clang,
				Name                = "webgpu",
				Sources             = new[] { "/path/to/header.h" },
				NameContainer = new NameContainer {
					ClassName = "WebGPULibraryNameContainer",
					Android   = "libapi.so",
					IOS       = "libapi.dylib",
					Linux     = "libapi.so",
					MacOS     = "libapi.dylib",
					Windows64 = "api.dll",
					Windows86 = "api.dll"
				},
				OutputOpts = new OutputOptions {
					Folder  = "/path/to/outputFolder",
					License = "/path/to/licenseFile",
					Props   = "msbuild.props"
				},
				TypeMaps = new List<Dictionary<string, string>> {
					new Dictionary<string, string> {
						{ "HWND", "nint" }
					},
					new Dictionary<string, string> {
						{ "$include.commonTypeMap", "csharp_typemap.json" } //include the standard C# typemap
					}
				}
			}
		}
	};

	public static string ExampleBuildProps = @"
<Project>
    <ItemGroup>
		<PackageReference Include=""Silk.NET.SilkTouch"" Version=""2.16.0"" />
		<PackageReference Include=""Silk.NET.Core"" Version=""2.16.0"" />
	</ItemGroup>
</Project>";

	public static void Main(string[] args) {
		//If no arguments are passed, tell the user how to make an example config file
		if (args.Length == 0) {
			Console.WriteLine("You need to provide a configuration file to the app! " +
							  "Type `buildtools example` to generate an example configuration file.");

			return;
		}

		if (args[0] == "example") {
			const string exampleConfigPath = "generator.json";

			Console.WriteLine("Writing example config...");
			File.WriteAllText(exampleConfigPath, JsonConvert.SerializeObject(ExampleJsonFile, Formatting.Indented, new JsonSerializerSettings() {
				NullValueHandling = NullValueHandling.Ignore, 
				DefaultValueHandling = DefaultValueHandling.Ignore
			}));

			Console.WriteLine("Writing default typemap...");
			File.WriteAllText("csharp_typemap.json", DefaultTypemaps.CSTypemap.Value);

			Console.WriteLine("Writing default MSBuild props...");
			File.WriteAllText("msbuild.props", ExampleBuildProps);

			Console.WriteLine($"Example config written to `{exampleConfigPath}`! " +
							  "Edit this file to your liking and then run `buildtools {path to your config file}`");

			return;
		}

		string configPath = string.Join(' ', args);
		
		Config generatorConfig = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath));

		Environment.CurrentDirectory = Path.GetDirectoryName(Path.GetFullPath(configPath)) ?? throw new Exception("bro wtf");
		
		Console.WriteLine($"Running generator for config file {configPath}...");
		
		Stopwatch sw = Stopwatch.StartNew();
		
		Generator.RunTask(generatorConfig.Tasks[0]);
		
		// Generator.Run(generatorConfig);
		
		sw.Stop();
		Console.WriteLine($"Finished in {Math.Floor(sw.Elapsed.TotalMinutes):N0}{sw.Elapsed.TotalSeconds % 60:N2}s");
	}
}
