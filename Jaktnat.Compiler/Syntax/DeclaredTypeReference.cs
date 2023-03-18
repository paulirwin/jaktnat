namespace Jaktnat.Compiler.Syntax;

public class DeclaredTypeReference : TypeReference
{
    public DeclaredTypeReference(TypeDeclarationSyntax declaredType)
    {
        DeclaredType = declaredType;
    }

    public TypeDeclarationSyntax DeclaredType { get; }

    public override string Name => DeclaredType.Name;
    
    public override string FullName => DeclaredType.Name; // TODO: support namespaces
    
    public override bool IsValueType => DeclaredType.IsValueType;

    public override bool IsArray => false; // TODO: support arrays

    public override bool IsVoid => false;

    public override TypeReference? GetElementType()
    {
        return null; // TODO: support arrays
    }

    public override string ToString() => DeclaredType.ToString();

    public override int GetHashCode() => DeclaredType.GetHashCode();

    public override bool Equals(Type? other) => false;

    public override bool Equals(TypeDeclarationSyntax? other) => DeclaredType.Equals(other); // TODO: support inheritance hierarchy

    protected bool Equals(DeclaredTypeReference other) => DeclaredType.Equals(other.DeclaredType);

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((DeclaredTypeReference)obj);
    }

    public override TypeReference MakeArrayType()
    {
        // TODO: support arrays
        throw new NotImplementedException();
    }
}