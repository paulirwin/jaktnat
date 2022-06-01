using Jaktnat.Compiler.Syntax;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Jaktnat.Compiler.ILGenerators;

internal static class TopLevelFunctionILGenerator
{
    public static void GenerateTopLevelFunction(CompilationContext context, FunctionSyntax function, TypeDefinition programClass)
    {
        bool isMain = function.Name.Equals("main");
        string name = isMain ? "Main" : function.Name;

        // FIXME: support return types other than void
        var voidType = programClass.Module.ImportReference(typeof(void));
        var method = new MethodDefinition(name, MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.HideBySig, voidType);

        programClass.Methods.Add(method);

        if (isMain)
        {
            var objArrayType = programClass.Module.ImportReference(typeof(string[]));
            method.Parameters.Add(new ParameterDefinition("args", ParameterAttributes.None, objArrayType));
            programClass.Module.Assembly.EntryPoint = method;
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