using Jaktnat.Compiler;

namespace Jaktnat;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: Jaktnat.exe {path}");
            return;
        }

        var fileText = File.ReadAllText(args[0]);

        var assemblyName = Path.GetFileNameWithoutExtension(args[0]);

        var options = new JaktnatCompilerOptions
        {
            Backend = BackendType.Roslyn,
        };

        JaktnatCompiler.CompileText(options, assemblyName, fileText);

        Console.WriteLine("Build succeeded");
    }
}