namespace Jaktnat.Compiler.Syntax;

public abstract class TypeReference : IEquatable<Type>, IEquatable<TypeDeclarationSyntax>
{
    public abstract string FullName { get; }

    public abstract bool IsValueType { get; }
    
    public abstract bool IsArray { get; }

    public abstract TypeReference? GetElementType();

    public abstract override string ToString();

    public abstract override int GetHashCode();

    public abstract bool Equals(Type? other);

    public abstract bool Equals(TypeDeclarationSyntax? other);

    public abstract override bool Equals(object? obj);

    public abstract TypeReference MakeArrayType();

    public static implicit operator TypeReference(Type runtimeType)
    {
        return new RuntimeTypeReference(runtimeType);
    }

    public static implicit operator TypeReference(TypeDeclarationSyntax declaredType)
    {
        return new DeclaredTypeReference(declaredType);
    }
}