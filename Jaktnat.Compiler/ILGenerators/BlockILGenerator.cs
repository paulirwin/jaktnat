using Jaktnat.Compiler.Syntax;
using Mono.Cecil.Cil;

namespace Jaktnat.Compiler.ILGenerators;

internal static class BlockILGenerator
{
    public static void GenerateBlock(CompilationContext context, ILProcessor il, BlockSyntax block)
    {
        foreach (var child in block.Children)
        {
            if (child is CallSyntax callSyntax)
            {
                FunctionCallILGenerator.GenerateFunctionCall(context, il, callSyntax);
            }
            else if (child is IfSyntax ifSyntax)
            {
                IfSyntaxILGenerator.GenerateIfStatement(context, il, ifSyntax);
            }
            else if (child is WhileSyntax whileSyntax)
            {
                WhileSyntaxILGenerator.GenerateWhileStatement(context, il, whileSyntax);
            }
            else if (child is VariableDeclarationSyntax varDecl)
            {
                VariableDeclarationILGenerator.GenerateVariableDeclaration(context, il, varDecl);
            }
        }
    }
}