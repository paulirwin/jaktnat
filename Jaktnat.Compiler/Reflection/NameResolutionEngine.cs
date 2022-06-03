using System.Reflection;
using Jaktnat.Compiler.Syntax;

namespace Jaktnat.Compiler.Reflection;

internal static class NameResolutionEngine
{
    public static void Resolve(CompilationContext context, SyntaxNode node)
    {
        if (node is CompositeSyntax composite)
        {
            foreach (var child in composite.Children)
            {
                Resolve(context, child);
            }
        }
        else if (node is FunctionSyntax function)
        {
            Resolve(context, function.Body);
        }
        else if (node is IfSyntax ifSyntax)
        {
            Resolve(context, ifSyntax.Condition);
            Resolve(context, ifSyntax.Body);
        }
        else if (node is CallSyntax call)
        {
            call.PossibleMatchedMethods = FreeFunctionResolver.Resolve(context, call.Name);
            
            foreach (var argument in call.Arguments)
            {
                Resolve(context, argument);
            }

            call.MatchedMethod = ResolveOverloads(call);

            if (call.MatchedMethod == null)
            {
                throw new CompilerError($"Unable to resolve method overload for {call.Name}");
            }
            else
            {
                call.ExpressionType = call.MatchedMethod.ReturnType;
            }
        }
        else if (node is CallArgumentSyntax argument)
        {
            Resolve(context, argument.Expression);
            argument.ArgumentType = argument.Expression.ExpressionType;
        }
        else if (node is VariableDeclarationSyntax varDecl)
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
                Resolve(context, varDecl.InitializerExpression);
                varDecl.Type = varDecl.InitializerExpression.ExpressionType;
                varDecl.ParentBlock.Variables.Add(varDecl.Name, varDecl);
            }

            if (varDecl.TypeName != null)
            {
                // TODO: resolve explicit type
            }
        }
        else if (node is IdentifierExpressionSyntax identifier)
        {
            if (identifier.ParentBlock == null)
            {
                throw new CompilerError($"Unable to resolve identifier '{identifier.Name}'");
            }

            if (identifier.ParentBlock.TryResolveVariable(identifier.Name, out varDecl))
            {
                identifier.ExpressionType = varDecl.Type;
            }
            else
            {
                throw new CompilerError($"Unable to resolve identifier '{identifier.Name}'");
            }
        }
    }

    private static MethodInfo? ResolveOverloads(CallSyntax call)
    {
        if (call.PossibleMatchedMethods == null)
        {
            return null;
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

        return null;
    }

    private static bool ParameterTypesAreCompatible(IReadOnlyList<Type> methodTypes, IReadOnlyList<Type?> expectedTypes)
    {
        // TODO: support params
        if (methodTypes.Count != expectedTypes.Count)
        {
            return false;
        }

        for (var index = 0; index < methodTypes.Count; index++)
        {
            var methodType = methodTypes[index];
            var expectedType = expectedTypes[index];

            if (expectedType == null || !methodType.IsAssignableFrom(expectedType))
            {
                return false;
            }
        }

        return true;
    }
}