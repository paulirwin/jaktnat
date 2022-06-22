using Jaktnat.Compiler.Syntax;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Jaktnat.Compiler.ILGenerators;

internal static class WhileSyntaxILGenerator
{
    public static void GenerateWhileStatement(CompilationContext context, ILProcessor il, WhileSyntax whileSyntax)
    {
        var conditionType = il.Body.Method.DeclaringType.Module.ImportReference(typeof(bool));
        var conditionVariable = new VariableDefinition(conditionType);

        il.Body.InitLocals = true;
        il.Body.Variables.Add(conditionVariable);

        var conditionLabel = il.Create(OpCodes.Nop);
        il.Append(il.Create(OpCodes.Br_S, conditionLabel));

        var jumpLabel = il.Create(OpCodes.Nop);
        il.Append(jumpLabel);
        
        BlockILGenerator.GenerateBlock(context, il, whileSyntax.Body);

        il.Append(conditionLabel);
        
        ExpressionILGenerator.GenerateExpression(context, il, whileSyntax.Condition, typeof(bool));

        il.Append(il.Create(OpCodes.Stloc_S, conditionVariable));

        il.Append(il.Create(OpCodes.Ldloc_S, conditionVariable));
        il.Append(il.Create(OpCodes.Brtrue_S, jumpLabel));

        il.Body.OptimizeMacros();
    }
}