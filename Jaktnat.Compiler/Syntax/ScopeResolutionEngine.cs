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

                if (ifSyntax.ElseNode != null)
                {
                    ResolveScopes(ifSyntax.ElseNode, parentBlock);
                }

                break;

            case ElseSyntax elseSyntax:
                ResolveScopes(elseSyntax.Child, parentBlock);
                break;
            case WhileSyntax whileSyntax:
                ResolveScopes(whileSyntax.Condition, parentBlock);
                ResolveScopes(whileSyntax.Body, parentBlock);
                break;
            case LoopSyntax loopSyntax:
                ResolveScopes(loopSyntax.Body, parentBlock);
                break;
            case FunctionSyntax functionSyntax:
                ResolveScopes(functionSyntax.Body, parentBlock);

                // special case: use body block as "parent" block for function params
                if (functionSyntax.Parameters != null)
                {
                    ResolveScopes(functionSyntax.Parameters, functionSyntax.Body);
                }
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
            case IndexerAccessSyntax indexer:
                ResolveScopes(indexer.Target, parentBlock);
                ResolveScopes(indexer.Argument, parentBlock);
                break;
            case ArraySyntax array:
                ResolveScopes(array.ItemsList, parentBlock);
                break;
            case ExpressionListSyntax expList:
                foreach (var item in expList.Items)
                {
                    ResolveScopes(item, parentBlock);
                }

                break;
            case UnaryExpressionSyntax unaryExpression:
                ResolveScopes(unaryExpression.Expression, parentBlock);
                break;
            case BinaryExpressionSyntax binaryExpression:
                ResolveScopes(binaryExpression.Left, parentBlock);
                ResolveScopes(binaryExpression.Right, parentBlock);
                break;
            case ParenthesizedExpressionSyntax parenthesizedExpression:
                ResolveScopes(parenthesizedExpression.Expression, parentBlock);
                break;
            case MemberAccessSyntax memberAccess:
                ResolveScopes(memberAccess.Target, parentBlock);
                ResolveScopes(memberAccess.Member, parentBlock);
                break;
            case ParameterListSyntax parameterList:
                foreach (var parameter in parameterList.Parameters)
                {
                    ResolveScopes(parameter, parentBlock);
                }

                break;
            case ClassDeclarationSyntax classDeclaration:
                foreach (var member in classDeclaration.Members)
                {
                    ResolveScopes(member, parentBlock);
                }

                break;
            case IdentifierExpressionSyntax:
            case LiteralExpressionSyntax:
            case ParameterSyntax:
            case PropertySyntax:
            case BreakSyntax:
            case ContinueSyntax:
            case ReturnSyntax:
                break; // nothing to do
            default:
                throw new NotImplementedException($"Scope resolution not implemented for syntax type {node.GetType()}");
        }
    }
}