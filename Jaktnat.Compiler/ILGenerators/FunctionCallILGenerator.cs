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
            throw new CompilerError($"Unable to resolve function \"{callSyntax.Target}\"");
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

                ExpressionILGenerator.GenerateExpression(context, il, argument.Expression, param.ParameterType);
            }
        }

        il.Append(il.Create(OpCodes.Call, methodRef));
    }
}