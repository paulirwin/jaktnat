using Jaktnat.Compiler.Syntax;
using Mono.Cecil.Cil;

namespace Jaktnat.Compiler.ILGenerators;

internal static class VariableDeclarationILGenerator
{
    public static void GenerateVariableDeclaration(CompilationContext context, ILProcessor il, VariableDeclarationSyntax varDecl)
    {
        if (varDecl.Type is not RuntimeTypeReference { RuntimeType: Type runtimeType })
        {
            // TODO: support declared types
            throw new CompilerError("Variable declaration type is not a runtime type");
        }

        var variableType = il.Body.Method.DeclaringType.Module.ImportReference(runtimeType);
        var variable = new VariableDefinition(variableType);

        il.Body.InitLocals = true;
        il.Body.Variables.Add(variable);

        varDecl.ILVariableDefinition = variable;

        if (varDecl.InitializerExpression != null)
        {
            ExpressionILGenerator.GenerateExpression(context, il, varDecl.InitializerExpression, varDecl.Type);
        }
        else
        {
            throw new NotImplementedException("Support for variable declarations without initializer expressions is not yet implemented");
        }

        il.Append(il.Create(OpCodes.Stloc_S, variable));
    }
}