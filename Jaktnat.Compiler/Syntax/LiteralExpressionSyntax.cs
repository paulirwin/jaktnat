using System.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace Jaktnat.Compiler.Syntax;

public class LiteralExpressionSyntax : ExpressionSyntax
{
    public LiteralExpressionSyntax(string value)
        : base(typeof(string))
    {
        Value = UnescapeStringLiteral(value);
    }

    public LiteralExpressionSyntax(object value)
        : base(value.GetType())
    {
        Value = value;
    }

    public object Value { get; }

    public override string ToString() => Value switch
    {
        string s => SymbolDisplay.FormatLiteral(s, true),
        char c => SymbolDisplay.FormatLiteral(c, true),
        _ => Value.ToString() ?? "",
    };

    public override bool Mutates => false;

    public override bool PreventsMutation => true; // literals are immutable
    
    private static string UnescapeStringLiteral(string value)
    {
        var sb = new StringBuilder();

        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];

            if (c == '\\')
            {
                if (i == value.Length - 1)
                {
                    throw new InvalidOperationException("Unable to parse escape sequence");
                }
                
                sb.Append(value[i + 1] switch
                {
                    '\\' => '\\',
                    'r' => '\r',
                    'n' => '\n',
                    't' => '\t',
                    '"' => '"',
                    '\'' => '\'',
                    _ => throw new NotSupportedException($"Escape sequence not supported"),
                });

                i += 1;
            }
            else
            {
                sb.Append(c);
            }
        }
        
        return sb.ToString();
    }
}