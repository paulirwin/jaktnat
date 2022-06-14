using Jaktnat.Compiler.ObjectModel;

namespace Jaktnat.Compiler.Syntax;

public class CompilationUnitSyntax : AggregateSyntax
{
    public IDictionary<string, object> Globals { get; } = new Dictionary<string, object>();

    internal void DeclareFreeFunction(string name, FreeFunction function)
    {
        if (Globals.TryGetValue(name, out var global))
        {
            switch (global)
            {
                case FreeFunction existingFunction:
                    Globals[name] = FreeFunctionOverloadSet.FromPair(existingFunction, function);
                    break;
                case FreeFunctionOverloadSet set:
                    set.FreeFunctions.Add(function);
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

    public override string ToString() => string.Join(Environment.NewLine + Environment.NewLine, Children);
}