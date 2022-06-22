using System.Reflection;
using Jaktnat.Compiler.Syntax;

namespace Jaktnat.Compiler.Backends;

internal interface ICompilerBackend
{
    Assembly CompileAssembly(CompilationContext context, string assemblyName, CompilationUnitSyntax compilationUnit);
}