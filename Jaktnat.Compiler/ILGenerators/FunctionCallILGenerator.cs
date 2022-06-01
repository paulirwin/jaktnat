using Jaktnat.Compiler.Reflection;
using Jaktnat.Compiler.Syntax;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Jaktnat.Compiler.ILGenerators;

internal static class FunctionCallILGenerator
{
    public static void GenerateFunctionCall(CompilationContext context, ILProcessor il, CallSyntax callSyntax)
    {
        // TODO: support calls other than top-level runtime functions
        var method = TopLevelFunctionResolver.Resolve(context, callSyntax.Name);

        if (method == null)
        {
            throw new CompilerError($"Unable to resolve function \"{callSyntax.Name}\"");
        }

        var methodRef = il.Body.Method.DeclaringType.Module.ImportReference(method);

        foreach (var argument in callSyntax.Arguments)
        {
            if (argument.Expression is LiteralExpressionSyntax literal)
            {
                if (literal.Value is string str)
                {
                    il.Append(il.Create(OpCodes.Ldstr, str));
                }
                else
                {
                    throw new NotImplementedException("Passing anything other than a string literal as a function argument is not yet implemented");
                }
            }
            else
            {
                throw new NotSupportedException("Passing anything other than a literal as a function argument is not yet implemented");
            }
        }

        il.Append(il.Create(OpCodes.Call, methodRef));
    }
}