using Jaktnat.Compiler.Syntax;

namespace Jaktnat.Compiler.ObjectModel;

internal abstract class Function
{
    public abstract TypeReference ReturnType { get; }
}