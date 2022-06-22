using Jaktnat.Compiler.Syntax;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Jaktnat.Compiler.ILGenerators;

internal static class TopLevelFunctionILGenerator
{
    public static void GenerateTopLevelFunction(CompilationContext context, FunctionSyntax function)
    {
        if (context.ProgramClass == null)
        {
            throw new InvalidOperationException("Program class not yet generated");
        }

        bool isMain = function.Name.Equals("main");
        string name = isMain ? "Main" : function.Name;

        // FIXME: support return types other than void
        var voidType = context.ProgramClass.Module.ImportReference(typeof(void));
        var method = new MethodDefinition(name, MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.HideBySig, voidType);

        context.ProgramClass.Methods.Add(method);

        if (isMain)
        {
            var objArrayType = context.ProgramClass.Module.ImportReference(typeof(string[]));
            method.Parameters.Add(new ParameterDefinition("args", ParameterAttributes.None, objArrayType));
            context.ProgramClass.Module.Assembly.EntryPoint = method;
        }
        else if (function.Parameters != null)
        {
            foreach (var param in function.Parameters.Parameters)
            {
                if (param.Type == null)
                {
                    throw new CompilerError($"Parameter {param.Name} does not have a resolved type");
                }

                var paramType = context.ProgramClass.Module.ImportReference(param.Type);
                method.Parameters.Add(new ParameterDefinition(param.Name, ParameterAttributes.None, paramType));
            }
        }

        var il = method.Body.GetILProcessor();

        if (function.Body is not BlockSyntax block)
        {
            throw new NotImplementedException("Only block syntax is supported for functions at this time");
        }

        BlockILGenerator.GenerateBlock(context, il, block);

        il.Append(il.Create(OpCodes.Ret));
    }
}