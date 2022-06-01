using Jaktnat.Compiler;

namespace Jaktnat;

internal class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: Jaktnat.exe {path}");
            return;
        }

        var compiler = new JaktnatCompiler();

        var fileText = File.ReadAllText(args[0]);

        var assemblyName = Path.GetFileNameWithoutExtension(args[0]);

        compiler.CompileText(assemblyName, fileText);

        Console.WriteLine("Build succeeded");
    }
}