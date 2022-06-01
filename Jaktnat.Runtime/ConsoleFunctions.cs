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

    [FreeFunction("println")]
    public static void Print(string format, object value)
    {
        PrintInternal(format, new[] { value }, false);
    }

    private static void PrintInternal(string format, object[] values, bool newline)
    {
        var sb = new StringBuilder();
        int valueIndex = 0;

        for (int i = 0; i < format.Length; i++)
        {
            char c = format[i];

            if (c == '{' && format[i + 1] == '}')
            {
                var value = values[valueIndex++] switch
                {
                    double d => d.ToString("0.######"),
                    float f => f.ToString("0.######"),
                    object o => o
                };

                sb.Append(value);
                i++;
            }
            else
            {
                sb.Append(c);
            }
        }

        if (newline)
        {
            sb.Append(Environment.NewLine);
        }

        Console.Write(sb.ToString());
    }
}