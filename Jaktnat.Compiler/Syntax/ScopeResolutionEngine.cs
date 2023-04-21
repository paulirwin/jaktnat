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
            case GuardSyntax guardSyntax:
                ResolveScopes(guardSyntax.Condition, parentBlock);
                ResolveScopes(guardSyntax.ElseNode, parentBlock);

                if (guardSyntax.ElseNode.Child is not (ReturnSyntax
                    or ThrowSyntax or BreakSyntax or ContinueSyntax
                    or BlockSyntax { PotentiallyExitsScope: true }))
                {
                    throw new CompilerError("Else block of guard must either `return`, `break`, `continue`, or `throw`");
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
                ResolveScopes(functionSyntax.Parameters, functionSyntax.Body);

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
                if (memberAccess.Target is { } target)
                {
                    ResolveScopes(target, parentBlock);
                }

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
            case StructDeclarationSyntax structDeclaration:
                foreach (var member in structDeclaration.Members)
                {
                    ResolveScopes(member, parentBlock);
                }

                break;
            case TrySyntax trySyntax:
                ResolveScopes(trySyntax.Tryable, parentBlock);
                ResolveScopes(trySyntax.Catch, parentBlock);
                break;
            case CatchSyntax catchSyntax:
                ResolveScopes(catchSyntax.Identifier, catchSyntax.CatchBlock);
                ResolveScopes(catchSyntax.CatchBlock, parentBlock);
                break;
            case ThrowSyntax throwSyntax:
                if (throwSyntax.Expression != null)
                {
                    ResolveScopes(throwSyntax.Expression, parentBlock);
                }
                
                MarkParentBlocksAsPotentiallyExitingScope(throwSyntax);
                break;
            case ReturnSyntax returnSyntax:
                if (returnSyntax.Expression != null)
                {
                    ResolveScopes(returnSyntax.Expression, parentBlock);
                }

                MarkParentBlocksAsPotentiallyExitingScope(returnSyntax);
                break;
            case ScopeAccessSyntax scopeAccess:
                ResolveScopes(scopeAccess.Scope, parentBlock);
                ResolveScopes(scopeAccess.Member, parentBlock);
                break;
            case MemberFunctionDeclarationSyntax memberFunction:
                ResolveScopes(memberFunction.Function, parentBlock);
                break;
            case DeferSyntax deferSyntax: 
                ResolveScopes(deferSyntax.Body, parentBlock);
                break;
            case CSharpBlockSyntax csharpBlock:
                ResolveScopes(csharpBlock.Block, parentBlock);
                break;
            case UnsafeBlockSyntax unsafeBlock:
                ResolveScopes(unsafeBlock.Block, parentBlock);
                
                foreach (var child in unsafeBlock.Block.Children)
                {
                    if (child is CSharpBlockSyntax csharpBlock)
                    {
                        csharpBlock.ParentUnsafeBlock = unsafeBlock;
                    }
                }
                
                break;
            case ForInSyntax forInSyntax:
                ResolveScopes(forInSyntax.Identifier, forInSyntax.Block);
                ResolveScopes(forInSyntax.Expression, parentBlock);
                ResolveScopes(forInSyntax.Block, parentBlock);
                break;
            case IdentifierExpressionSyntax:
            case BlockScopedIdentifierSyntax:
            case LiteralExpressionSyntax:
            case ParameterSyntax:
            case PropertySyntax:
            case BreakSyntax:
                MarkParentBlocksAsPotentiallyExitingScope(node);
                break;
            case ContinueSyntax:
                MarkParentBlocksAsPotentiallyExitingScope(node);
                break;
            case ThisExpressionSyntax:
                break; // nothing to do
            default:
                throw new NotImplementedException($"Scope resolution not implemented for syntax type {node.GetType()}");
        }
    }

    private static void MarkParentBlocksAsPotentiallyExitingScope(SyntaxNode node)
    {
        BlockSyntax? blockSyntax = node.ParentBlock;

        while (blockSyntax != null)
        {
            blockSyntax.PotentiallyExitsScope = true;
            blockSyntax = blockSyntax.ParentBlock;
        }
    }
}