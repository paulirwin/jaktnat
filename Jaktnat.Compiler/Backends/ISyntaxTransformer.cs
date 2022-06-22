using Jaktnat.Compiler.Syntax;

namespace Jaktnat.Compiler.Backends;

internal interface ISyntaxTransformer<out T>
{
    T Visit(CompilationContext context, SyntaxNode node);
}