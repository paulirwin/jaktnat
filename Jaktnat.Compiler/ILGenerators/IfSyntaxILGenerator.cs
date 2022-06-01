using Jaktnat.Compiler.Syntax;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Jaktnat.Compiler.ILGenerators;

internal static class IfSyntaxILGenerator
{
    public static void GenerateIfStatement(CompilationContext context, ILProcessor il, IfSyntax ifSyntax)
    {
        ExpressionILGenerator.GenerateExpression(context, il, ifSyntax.Condition, typeof(bool));

        var elseLabel = il.Create(OpCodes.Nop);
        il.Append(il.Create(OpCodes.Brfalse, elseLabel));

        BlockILGenerator.GenerateBlock(context, il, ifSyntax.Block);

        il.Append(elseLabel);

        il.Body.OptimizeMacros();
    }
}