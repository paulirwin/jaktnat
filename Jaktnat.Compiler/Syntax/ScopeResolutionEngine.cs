namespace Jaktnat.Compiler.Syntax;

internal static class ScopeResolutionEngine
{
    public static void ResolveScopes(SyntaxNode node, BlockSyntax? parentBlock)
    {
        node.ParentBlock = parentBlock;

        switch (node)
        {
            case CompilationUnitSyntax compilationUnit:
                // NOTE: if support for top-level variables is added later, this could change
                foreach (var child in compilationUnit.Children)
                {
                    ResolveScopes(child, parentBlock);
                }
                break;
            case BlockSyntax block:
                foreach (var child in block.Children)
                {
                    ResolveScopes(child, block);
                }
                break;
            case IfSyntax ifSyntax:
                ResolveScopes(ifSyntax.Condition, parentBlock);
                ResolveScopes(ifSyntax.Body, parentBlock);
                break;
            case FunctionSyntax functionSyntax:
                ResolveScopes(functionSyntax.Body, parentBlock);
                break;
            case CallArgumentSyntax callArgumentSyntax:
                ResolveScopes(callArgumentSyntax.Expression, parentBlock);
                break;
            case CallSyntax callSyntax:
                ResolveScopes(callSyntax.Target, parentBlock);
                foreach (var argument in callSyntax.Arguments)
                {
                    ResolveScopes(argument, parentBlock);
                }
                break;
            case VariableDeclarationSyntax variableDeclarationSyntax:
                if (variableDeclarationSyntax.InitializerExpression != null)
                {
                    ResolveScopes(variableDeclarationSyntax.InitializerExpression, parentBlock);
                }

                if (parentBlock == null)
                {
                    throw new InvalidOperationException("Cannot define a variable without a parent block");
                }
                
                break;
            case IdentifierExpressionSyntax:
            case LiteralExpressionSyntax:
                break; // nothing to do
            default:
                throw new NotImplementedException($"Scope resolution not implemented for syntax type {node.GetType()}");
        }
    }
}