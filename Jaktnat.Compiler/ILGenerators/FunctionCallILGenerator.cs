using System.Reflection;
using Jaktnat.Compiler.Syntax;
using Mono.Cecil.Cil;

namespace Jaktnat.Compiler.ILGenerators;

internal static class FunctionCallILGenerator
{
    public static void GenerateFunctionCall(CompilationContext context, ILProcessor il, CallSyntax callSyntax)
    {
        if (callSyntax.MatchedMethod is not { } method)
        {
            throw new CompilerError($"Unable to resolve function \"{callSyntax.Name}\"");
        }

        var methodRef = il.Body.Method.DeclaringType.Module.ImportReference(method);
        var parameters = method.GetParameters();

        for (var paramIndex = 0; paramIndex < parameters.Length; paramIndex++)
        {
            var param = parameters[paramIndex];
            int stop = paramIndex + 1;

            if (param.ParameterType.IsArray && param.GetCustomAttribute<ParamArrayAttribute>() != null)
            {
                // params parameter; consume the rest of the arguments into the array
                stop = callSyntax.Arguments.Count;

                throw new NotImplementedException("Calling variable-argument \"params\" functions is not yet supported");
            }

            for (int argIndex = paramIndex; argIndex < stop; argIndex++)
            {
                var argument = callSyntax.Arguments[argIndex];
                if (argument.Expression is LiteralExpressionSyntax literal)
                {
                    if (literal.ExpressionType == param.ParameterType)
                    {
                        var op = GetLoadLiteralOp(il, literal);

                        il.Append(op);
                    }
                    else if (literal.ExpressionType.IsValueType && param.ParameterType == typeof(object))
                    {
                        var tempType = il.Body.Method.DeclaringType.Module.ImportReference(literal.ExpressionType);
                        
                        il.Append(GetLoadLiteralOp(il, literal)); // put literal on stack
                        il.Append(il.Create(OpCodes.Box, tempType)); // constrain (box) value type to parameter type
                    }
                }
                else
                {
                    throw new NotSupportedException("Passing anything other than a literal as a function argument is not yet implemented");
                }
            }
        }

        il.Append(il.Create(OpCodes.Call, methodRef));
    }

    private static Instruction GetLoadLiteralOp(ILProcessor il, LiteralExpressionSyntax literal)
    {
        var op = literal.Value switch
        {
            string str => il.Create(OpCodes.Ldstr, str),
            long l => il.Create(OpCodes.Ldc_I8, l),
            double d => il.Create(OpCodes.Ldc_R8, d),
            _ => throw new NotImplementedException($"Passing a {literal.Value.GetType()} literal as a function argument is not yet implemented"),
        };
        return op;
    }
}