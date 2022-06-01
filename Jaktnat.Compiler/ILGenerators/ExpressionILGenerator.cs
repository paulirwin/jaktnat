using Jaktnat.Compiler.Syntax;
using Mono.Cecil.Cil;

namespace Jaktnat.Compiler.ILGenerators;

internal static class ExpressionILGenerator
{
    public static void GenerateExpression(CompilationContext context, ILProcessor il, ExpressionSyntax expression, Type? targetType)
    {
        if (expression is LiteralExpressionSyntax literal)
        {
            if (targetType == null || literal.ExpressionType == targetType)
            {
                var op = GetLoadLiteralOp(il, literal);

                il.Append(op);
            }
            else if (literal.ExpressionType.IsValueType && targetType == typeof(object))
            {
                var tempType = il.Body.Method.DeclaringType.Module.ImportReference(literal.ExpressionType);

                il.Append(GetLoadLiteralOp(il, literal)); // put literal on stack
                il.Append(il.Create(OpCodes.Box, tempType)); // constrain (box) value type to parameter type
            }
        }
        else
        {
            throw new NotSupportedException($"Expression type {expression.GetType()} is not yet implemented");
        }
    }

    private static Instruction GetLoadLiteralOp(ILProcessor il, LiteralExpressionSyntax literal)
    {
        var op = literal.Value switch
        {
            string str => il.Create(OpCodes.Ldstr, str),
            long l => il.Create(OpCodes.Ldc_I8, l),
            double d => il.Create(OpCodes.Ldc_R8, d),
            true => il.Create(OpCodes.Ldc_I4_1),
            false => il.Create(OpCodes.Ldc_I4_0),
            _ => throw new NotImplementedException($"Passing a {literal.Value.GetType()} literal as a function argument is not yet implemented"),
        };
        return op;
    }
}