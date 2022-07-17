using System.CommandLine;
using Jaktnat.Compiler;

namespace Jaktnat;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var rootCommand = new RootCommand("The Jaktnät programming language compiler");

        var pathArg = new Argument<FileInfo>("input", "The path to the source file to compile");

        var backendOption = new Option<BackendType>("--backend", "The compiler backend to use")
            .FromAmong(BackendType.Roslyn.ToString(), BackendType.MonoCecil.ToString());
        backendOption.SetDefaultValue(BackendType.Roslyn);

        var buildCommand = new Command("build", "Compiles Jaktnät code into a .NET executable");
        buildCommand.SetHandler(Build, pathArg, backendOption);
        rootCommand.Add(buildCommand);

        buildCommand.AddArgument(pathArg);
        buildCommand.AddOption(backendOption);
        
        var outputFileOption = new Option<FileInfo>("--output", "Write the C# source to the specified output path");
        outputFileOption.AddAlias("-o");
        
        var transpileCommand = new Command("transpile", "Transpiles Jaktnät code into C#");
        transpileCommand.AddArgument(pathArg);
        transpileCommand.AddOption(outputFileOption);
        transpileCommand.SetHandler(Transpile, pathArg, outputFileOption);
        rootCommand.Add(transpileCommand);

        await rootCommand.InvokeAsync(args);
    }

    private static void Build(FileInfo path, BackendType backendType)
    {
        var fileText = File.ReadAllText(path.FullName);

        var assemblyName = Path.GetFileNameWithoutExtension(path.FullName);

        var options = new BuildOptions
        {
            Backend = backendType,
        };

        JaktnatCompiler.CompileText(options, assemblyName, fileText);

        Console.WriteLine("Build succeeded");
    }
    
    private static void Transpile(FileInfo path, FileInfo? outputFile)
    {
        var fileText = File.ReadAllText(path.FullName);

        var assemblyName = Path.GetFileNameWithoutExtension(path.FullName);

        var options = new BuildOptions
        {
            Backend = BackendType.Roslyn,
        };

        var cSharpCode = JaktnatCompiler.TranspileText(options, assemblyName, fileText);

        if (outputFile != null)
        {
            File.WriteAllText(outputFile.FullName, cSharpCode);
        }
        else
        {
            Console.WriteLine(cSharpCode);
        }
    }
}