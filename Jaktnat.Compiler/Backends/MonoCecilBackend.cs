using System.Reflection;
using Jaktnat.Compiler.ILGenerators;
using Jaktnat.Compiler.Syntax;

namespace Jaktnat.Compiler.Backends;

internal class MonoCecilBackend : ICompilerBackend
{
    public Assembly CompileAssembly(CompilationContext context, string assemblyName, CompilationUnitSyntax compilationUnit)
    {
        return AssemblyGenerator.CompileAssembly(context, assemblyName, compilationUnit);
    }

    public bool CanTranspile => false;
    
    public string Transpile(CompilationContext context, CompilationUnitSyntax compilationUnit)
    {
        throw new InvalidOperationException("The Mono.Cecil backend does not support transpiling");
    }
}