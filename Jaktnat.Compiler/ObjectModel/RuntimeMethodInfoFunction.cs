using System.Reflection;
using Jaktnat.Compiler.Syntax;

namespace Jaktnat.Compiler.ObjectModel;

internal class RuntimeMethodInfoFunction : Function
{
    public RuntimeMethodInfoFunction(MethodInfo method)
    {
        Method = method;
    }

    public MethodInfo Method { get; }

    public override TypeReference ReturnType => Method.ReturnType;

    public override bool Mutates => true; // TODO: can we determine if runtime functions are immutable?

    public override string ToString() => Method.ToString() ?? "null";
}