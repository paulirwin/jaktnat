namespace Jaktnat.Compiler.Syntax;

public class RuntimeTypeReference : TypeReference
{
    public RuntimeTypeReference(Type runtimeType)
    {
        RuntimeType = runtimeType;
    }

    public Type RuntimeType { get; }

    public override string Name => RuntimeType.Name;
    
    public override string FullName => RuntimeType.FullName ?? throw new InvalidOperationException("Runtime type does not have a full name");
    
    public override bool IsValueType => RuntimeType.IsValueType;
    
    public override bool IsArray => RuntimeType.IsArray;

    public override TypeReference? GetElementType() => RuntimeType.GetElementType() is Type type ? new RuntimeTypeReference(type) : null;
    
    public override string ToString() => RuntimeType.ToString();

    public override int GetHashCode() => RuntimeType.GetHashCode();

    public override bool Equals(Type? other) => RuntimeType == other;

    public override bool Equals(TypeDeclarationSyntax? other) => false;

    protected bool Equals(RuntimeTypeReference other) => RuntimeType == other.RuntimeType;

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((RuntimeTypeReference)obj);
    }

    public override TypeReference MakeArrayType() => RuntimeType.MakeArrayType();
}