using Jaktnat.Compiler.Syntax;

namespace Jaktnat.Compiler;

internal interface ISyntaxVisitor<in T>
    where T : SyntaxNode
{
    virtual void PreVisit(CompilationContext context, T node) { }
    void Visit(CompilationContext context, T node);
}