using System.Reflection;

namespace Jaktnat.Compiler.ObjectModel;

internal class RuntimeMethodInfoFreeFunction : FreeFunction
{
    public RuntimeMethodInfoFreeFunction(MethodInfo method)
    {
        Method = method;
    }

    public MethodInfo Method { get; }

    public override Type ReturnType => Method.ReturnType;
}