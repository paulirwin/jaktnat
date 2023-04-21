using System.Text;

namespace Jaktnat.Runtime;

public static class StringFunctions
{
    [FreeFunction("format")]
    public static string Format(string format)
    {
        return format;
    }

    [FreeFunction("format")]
    public static string Format(string format, object? arg)
    {
        return FormatInternal(format, arg);
    }

    [FreeFunction("format")]
    public static string Format(string format, object? arg1, object? arg2)
    {
        return FormatInternal(format, arg1, arg2);
    }

    [FreeFunction("format")]
    public static string Format(string format, object? arg1, object? arg2, object? arg3)
    {
        return FormatInternal(format, arg1, arg2, arg3);
    }

    internal static string FormatInternal(string format, params object?[] args)
    {
        var sb = new StringBuilder();
        int valueIndex = 0;

        for (int i = 0; i < format.Length; i++)
        {
            char c = format[i];

            if (c == '{' && format[i + 1] == '}')
            {
                var arg = args[valueIndex++] switch
                {
                    double d => d.ToString("0.######"),
                    float f => f.ToString("0.######"),
                    object o => o,
                    null => "",
                };

                sb.Append(arg);
                i++;
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }
}