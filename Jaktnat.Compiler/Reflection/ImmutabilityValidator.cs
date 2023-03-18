using Jaktnat.Compiler.Syntax;

namespace Jaktnat.Compiler.Reflection;

internal class ImmutabilityValidator : 
    ISyntaxVisitor<CallSyntax>
{
    public void Visit(CompilationContext context, CallSyntax node)
    {
        if (!node.Mutates)
        {
            return;
        }
        
        if (node.Target.PreventsMutation)
        {
            throw new CompilerError("Cannot call mutating method on an immutable object instance");
        }
    }
}