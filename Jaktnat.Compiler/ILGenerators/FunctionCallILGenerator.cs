using System.Reflection;
using Jaktnat.Compiler.ObjectModel;
using Jaktnat.Compiler.Syntax;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Jaktnat.Compiler.ILGenerators;

internal static class FunctionCallILGenerator
{
    public static void GenerateFunctionCall(CompilationContext context, ILProcessor il, CallSyntax callSyntax)
    {
        if (context.ProgramClass == null)
        {
            throw new InvalidOperationException("Program class not yet generated");
        }

        MethodReference methodRef;
        var paramTypes = new List<Type>();

        if (callSyntax.MatchedMethod is RuntimeMethodBaseFunction { Method: MethodInfo method })
        {
            methodRef = il.Body.Method.DeclaringType.Module.ImportReference(method);

            var parameters = method.GetParameters();

            foreach (var param in parameters)
            {
                if (param.ParameterType.IsArray && param.GetCustomAttribute<ParamArrayAttribute>() != null)
                {
                    throw new NotImplementedException("Calling variable-argument \"params\" functions is not yet supported");
                }

                paramTypes.Add(param.ParameterType);
            }
        }
        else if (callSyntax.MatchedMethod is DeclaredFunction declaredFreeFunction)
        {
            var programMethod = context.ProgramClass.Methods.FirstOrDefault(i => i.Name == declaredFreeFunction.Name);

            methodRef = programMethod ?? throw new CompilerError($"Unable to resolve function \"{callSyntax.Target}\"");

            if (declaredFreeFunction.Function.Parameters != null)
            {
                foreach (var parameter in declaredFreeFunction.Function.Parameters.Parameters)
                {
                    // TODO: support declared types
                    if (parameter.Type is not RuntimeTypeReference { RuntimeType: Type runtimeType })
                    {
                        throw new CompilerError($"Missing resolved type for parameter {parameter.Name}");
                    }

                    paramTypes.Add(runtimeType);
                }
            }
        }
        else
        {
            throw new CompilerError($"Unable to resolve function \"{callSyntax.Target}\"");
        }
        
        for (var paramIndex = 0; paramIndex < paramTypes.Count; paramIndex++)
        {
            var param = paramTypes[paramIndex];
            int stop = paramIndex + 1;
            
            for (int argIndex = paramIndex; argIndex < stop; argIndex++)
            {
                var argument = callSyntax.Arguments[argIndex];

                ExpressionILGenerator.GenerateExpression(context, il, argument.Expression, param);
            }
        }

        il.Append(il.Create(OpCodes.Call, methodRef));
    }
}