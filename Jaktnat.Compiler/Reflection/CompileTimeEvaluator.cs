using Jaktnat.Compiler.Syntax;

namespace Jaktnat.Compiler.Reflection;

public static class CompileTimeEvaluator
{
    public static object Evaluate(ExpressionSyntax? expression)
    {
        return expression switch
        {
            LiteralExpressionSyntax literal => literal.Value,
            null => DBNull.Value, // this is what RawDefaultValue returns if there is no default value
            _ => throw new CompilerError($"Cannot evaluate {expression.GetType().Name} at compile time.")
        };
    }
}