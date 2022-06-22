using System.Reflection;
using Jaktnat.Compiler.ILGenerators;
using Jaktnat.Compiler.Syntax;

namespace Jaktnat.Compiler.Backends;

internal class ILGeneratorBackend : ICompilerBackend
{
    public Assembly CompileAssembly(CompilationContext context, string assemblyName, CompilationUnitSyntax compilationUnit)
    {
        return AssemblyGenerator.CompileAssembly(context, assemblyName, compilationUnit);
    }
}