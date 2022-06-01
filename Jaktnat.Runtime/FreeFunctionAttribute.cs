namespace Jaktnat.Runtime;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class FreeFunctionAttribute : Attribute
{
    public FreeFunctionAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; }
}