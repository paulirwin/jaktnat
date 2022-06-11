using System.Reflection;
using Jaktnat.Compiler.Syntax;

namespace Jaktnat.Compiler.Reflection;

internal class NameResolutionEngine : 
    ISyntaxVisitor<CallArgumentSyntax>,
    ISyntaxVisitor<VariableDeclarationSyntax>,
    ISyntaxVisitor<IdentifierExpressionSyntax>,
    ISyntaxVisitor<CallSyntax>
{
    public void Visit(CompilationContext context, CallArgumentSyntax node)
    {
        node.ArgumentType = node.Expression.ExpressionType;
    }

    public void Visit(CompilationContext context, VariableDeclarationSyntax varDecl)
    {
        if (varDecl.ParentBlock == null)
        {
            throw new CompilerError("Cannot define a variable outside of a block");
        }

        if (varDecl.ParentBlock.Variables.ContainsKey(varDecl.Name))
        {
            throw new CompilerError($"Variable '{varDecl.Name}' has already been declared in this scope");
        }

        if (varDecl.InitializerExpression != null)
        {
            varDecl.Type = varDecl.InitializerExpression.ExpressionType;
            varDecl.ParentBlock.Variables.Add(varDecl.Name, varDecl);
        }

        if (varDecl.TypeName != null)
        {
            // TODO: resolve explicit type
        }
    }

    public void Visit(CompilationContext context, IdentifierExpressionSyntax identifier)
    {
        if (identifier.ParentBlock == null)
        {
            throw new CompilerError($"Unable to resolve identifier '{identifier.Name}'");
        }

        if (identifier.ParentBlock.TryResolveVariable(identifier.Name, out var varDecl))
        {
            identifier.ExpressionType = varDecl.Type;
            identifier.Target = varDecl;
        }
        else if (FreeFunctionResolver.Resolve(context, identifier.Name) is { Count: > 0 } methods)
        {
            identifier.ExpressionType = typeof(MethodInfo);
            identifier.Target = methods;
        }
        else
        {
            throw new CompilerError($"Unable to resolve identifier '{identifier.Name}'");
        }
    }

    public void Visit(CompilationContext context, CallSyntax node)
    {
        if (node.Target is IdentifierExpressionSyntax { Target: IList<MethodInfo> methods })
        {
            node.PossibleMatchedMethods = methods;
        }

        node.MatchedMethod = ResolveOverloads(node);
        node.ExpressionType = node.MatchedMethod.ReturnType;
    }

    private static MethodInfo ResolveOverloads(CallSyntax call)
    {
        if (call.PossibleMatchedMethods == null)
        {
            throw new InvalidOperationException("Overload resolution called after name resolution");
        }

        var argTypes = call.Arguments.Select(i => i.ArgumentType).ToArray();

        // TODO: support matching on parameter name?
        // TODO: support `params` parameters
        // TODO: support overload precedence, i.e. for a String argument, prefer (String s) over (Object o)
        foreach (var possibility in call.PossibleMatchedMethods)
        {
            var parameters = possibility.GetParameters();

            if (parameters.Length == call.Arguments.Count
                && ParameterTypesAreCompatible(parameters.Select(i => i.ParameterType).ToArray(), argTypes))
            {
                return possibility;
            }
        }

        throw new CompilerError($"Unable to resolve overload. Possible matches: {string.Join(", ", call.PossibleMatchedMethods)}");
    }

    private static bool ParameterTypesAreCompatible(IReadOnlyList<Type> parameterTypes, IReadOnlyList<Type?> argumentTypes)
    {
        // TODO: support params
        if (parameterTypes.Count != argumentTypes.Count)
        {
            return false;
        }

        for (var index = 0; index < parameterTypes.Count; index++)
        {
            var parameterType = parameterTypes[index];
            var argumentType = argumentTypes[index];

            if (argumentType == null || !ParameterTypeIsCompatible(parameterType, argumentType))
            {
                return false;
            }
        }

        return true;
    }

    private static bool ParameterTypeIsCompatible(Type parameterType, Type argumentType)
    {
        return parameterType.IsAssignableFrom(argumentType)
               || (parameterType == typeof(object) && argumentType.IsValueType);
    }
}