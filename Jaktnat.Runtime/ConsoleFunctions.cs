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
        PrintlnInternal(format, new[] { value });
    }

    private static void PrintlnInternal(string format, object[] values)
    {
        var sb = new StringBuilder();
        int valueIndex = 0;

        for (int i = 0; i < format.Length; i++)
        {
            char c = format[i];

            if (c == '{' && format[i + 1] == '}')
            {
                sb.Append(values[valueIndex++]);
                i++;
            }
            else
            {
                sb.Append(c);
            }
        }

        Console.WriteLine(sb.ToString());
    }
}