using System.Text;

namespace Jaktnat.Runtime;

public static class ConsoleFunctions
{
    [FreeFunction("println")]
    public static void Println(string format)
    {
        Console.WriteLine(format);
    }

    [FreeFunction("println")]
    public static void Println(string format, object value)
    {
        PrintInternal(format, new[] { value }, true);
    }

    [FreeFunction("print")]
    public static void Print(string format)
    {
        Console.Write(format);
    }

    [FreeFunction("print")]
    public static void Print(string format, object arg)
    {
        PrintInternal(format, new[] { arg }, false);
    }

    private static void PrintInternal(string format, object[] args, bool newline)
    {
        var formatted = StringFunctions.FormatInternal(format, args);

        if (newline)
        {
            Console.WriteLine(formatted);
        }
        else
        {
            Console.Write(formatted);
        }
    }
}