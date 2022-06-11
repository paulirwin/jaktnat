using Jaktnat.Compiler.Syntax;

namespace Jaktnat.Compiler;

internal interface ISyntaxVisitor<in T>
    where T : SyntaxNode
{
    void Visit(CompilationContext context, T node);
}