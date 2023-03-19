using System.Reflection;
using Jaktnat.Compiler.Syntax;

namespace Jaktnat.Compiler.ObjectModel;

internal class RuntimeMethodBaseFunction : Function
{
    public RuntimeMethodBaseFunction(MethodBase method)
    {
        Method = method;
    }

    public MethodBase Method { get; }

    public override TypeReference ReturnType => Method switch
    {
        ConstructorInfo ctor => ctor.DeclaringType ?? throw new InvalidOperationException("Constructors must have a declaring type"),
        MethodInfo method => method.ReturnType,
        _ => throw new InvalidOperationException($"MethodBase type {Method.GetType().Name}")
    };

    public override bool Mutates => true; // TODO: can we determine if runtime functions are immutable?

    public override string ToString() => Method.ToString() ?? "null";
}