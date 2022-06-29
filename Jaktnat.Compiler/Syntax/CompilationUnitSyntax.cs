using Jaktnat.Compiler.ObjectModel;

namespace Jaktnat.Compiler.Syntax;

public class CompilationUnitSyntax : AggregateSyntax
{
    public IDictionary<string, object> Globals { get; } = new Dictionary<string, object>();

    internal void DeclareFreeFunction(string name, Function function)
    {
        if (Globals.TryGetValue(name, out var global))
        {
            switch (global)
            {
                case Function existingFunction:
                    Globals[name] = FunctionOverloadSet.FromPair(existingFunction, function);
                    break;
                case FunctionOverloadSet set:
                    set.Functions.Add(function);
                    break;
                default:
                    throw new CompilerError($"Cannot declare function {name} because it has already been declared in global scope as a {global.GetType()}");
            }
        }
        else
        {
            Globals.Add(name, function);
        }
    }

    internal void DeclareType(string name, TypeDeclarationSyntax typeSyntax)
    {
        if (Globals.ContainsKey(name))
        {
            throw new CompilerError($"The name {name} is already defined in this scope");
        }

        Globals.Add(name, typeSyntax);
    }

    public override string ToString() => string.Join(Environment.NewLine + Environment.NewLine, Children);

    public bool TryResolveType(string name, out TypeDeclarationSyntax? type)
    {
        if (Globals.TryGetValue(name, out var global) && global is TypeDeclarationSyntax typeDecl)
        {
            type = typeDecl;
            return true;
        }

        type = null;
        return false;
    }
}