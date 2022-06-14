using System.Reflection;
using Jaktnat.Compiler.ObjectModel;
using Jaktnat.Compiler.Syntax;

namespace Jaktnat.Compiler.Reflection;

internal class NameResolutionEngine : 
    ISyntaxVisitor<CallArgumentSyntax>,
    ISyntaxVisitor<VariableDeclarationSyntax>,
    ISyntaxVisitor<IdentifierExpressionSyntax>,
    ISyntaxVisitor<CallSyntax>,
    ISyntaxVisitor<FunctionSyntax>,
    ISyntaxVisitor<ArrayTypeIdentifierSyntax>,
    ISyntaxVisitor<NamedTypeIdentifierSyntax>,
    ISyntaxVisitor<ParameterSyntax>,
    ISyntaxVisitor<ArraySyntax>,
    ISyntaxVisitor<WhileSyntax>,
    ISyntaxVisitor<BinaryExpressionSyntax>,
    ISyntaxVisitor<UnaryExpressionSyntax>,
    ISyntaxVisitor<TypeCastSyntax>,
    ISyntaxVisitor<TypeCheckSyntax>
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
        else if (context.CompilationUnit.Globals.TryGetValue(identifier.Name, out var global))
        {
            // TODO: consider free functions in runtime assembly below equal?
            switch (global)
            {
                case FreeFunction function:
                    identifier.ExpressionType = typeof(MethodInfo);
                    identifier.Target = function;
                    break;
                case FreeFunctionOverloadSet overloadSet:
                    identifier.ExpressionType = typeof(MethodInfo);
                    identifier.Target = overloadSet;
                    break;
                default:
                    throw new CompilerError($"Globals of type {global.GetType()} are not supported");
            }
        }
        else
        {
            throw new CompilerError($"Unable to resolve identifier '{identifier.Name}'");
        }
    }

    public void Visit(CompilationContext context, CallSyntax node)
    {
        if (node.Target is IdentifierExpressionSyntax { Target: FreeFunction function })
        {
            node.PossibleMatchedMethods = new List<FreeFunction> { function };
        }
        else if (node.Target is IdentifierExpressionSyntax { Target: FreeFunctionOverloadSet overloadSet })
        {
            node.PossibleMatchedMethods = overloadSet.FreeFunctions;
        }

        node.MatchedMethod = ResolveOverloads(node);
        node.ExpressionType = node.MatchedMethod.ReturnType;
    }

    private static FreeFunction ResolveOverloads(CallSyntax call)
    {
        if (call.PossibleMatchedMethods == null)
        {
            throw new InvalidOperationException("Overload resolution called before name resolution");
        }

        var argTypes = call.Arguments.Select(i => i.ArgumentType).ToArray();

        // TODO: support matching on parameter name?
        // TODO: support `params` parameters
        // TODO: support overload precedence, i.e. for a String argument, prefer (String s) over (Object o)
        foreach (var possibility in call.PossibleMatchedMethods)
        {
            if (possibility is RuntimeMethodInfoFreeFunction runtimeFunction)
            {
                var parameterTypes = runtimeFunction.Method.GetParameters().Select(i => i.ParameterType).ToArray();

                if (parameterTypes.Length == call.Arguments.Count
                    && ParameterTypesAreCompatible(parameterTypes, argTypes))
                {
                    return possibility;
                }
            }
            else if (possibility is DeclaredFreeFunction declaredFunction)
            {
                if (declaredFunction.FunctionSyntax.Parameters == null
                    && call.Arguments.Count == 0)
                {
                    return possibility;
                }
                else if (call.Arguments.Count > 0 && declaredFunction.FunctionSyntax.Parameters is { } parameters)
                {
                    var parameterTypes = parameters.Parameters
                        .Select(i => i.ParameterType).ToArray();

                    if (parameterTypes.Length == call.Arguments.Count
                        && ParameterTypesAreCompatible(parameterTypes, argTypes))
                    {
                        return possibility;
                    }
                }
            }
        }

        throw new CompilerError($"Unable to resolve overload, ambiguous matches found: {string.Join(", ", call.PossibleMatchedMethods)}");
    }

    private static bool ParameterTypesAreCompatible(IReadOnlyList<Type?> parameterTypes, IReadOnlyList<Type?> argumentTypes)
    {
        // TODO: support params
        if (parameterTypes.Count != argumentTypes.Count)
        {
            return false;
        }
        
        if (parameterTypes.Count == 0)
        {
            return true;
        }

        for (var index = 0; index < parameterTypes.Count; index++)
        {
            var parameterType = parameterTypes[index];
            var argumentType = argumentTypes[index];

            if (argumentType == null || parameterType == null || !ParameterTypeIsCompatible(parameterType, argumentType))
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

    public void Visit(CompilationContext context, FunctionSyntax node)
    {
        node.ReturnType = typeof(void); // TODO: name resolve function return type

        context.CompilationUnit.DeclareFreeFunction(node.Name, new DeclaredFreeFunction(node));
    }

    public void Visit(CompilationContext context, ArrayTypeIdentifierSyntax node)
    {
        if (node.ElementType.Type == null)
        {
            throw new CompilerError("Element type is not defined for array type");
        }

        node.Type = node.ElementType.Type.MakeArrayType();
    }

    public void Visit(CompilationContext context, NamedTypeIdentifierSyntax node)
    {
        if (BuiltInTypeResolver.TryResolveType(node.Name) is Type type)
        {
            node.Type = type;
        }
        else
        {
            throw new CompilerError($"Unable to resolve named type: {node.Name}");
        }
    }

    public void Visit(CompilationContext context, ParameterSyntax node)
    {
        node.ParameterType = node.TypeIdentifier.Type;
    }

    public void Visit(CompilationContext context, ArraySyntax node)
    {
        if (node.ItemsList.Items.Any(i => i.ExpressionType == null))
        {
            throw new CompilerError("Unable to evaluate type for array syntax");
        }

        var distinctTypes = node.ItemsList.Items.DistinctBy(i => i.ExpressionType).ToList();

        // TODO: fix this
        if (distinctTypes.Count > 1)
        {
            throw new CompilerError("All items in an array expression must currently be the same type.");
        }

        node.ExpressionType = distinctTypes[0].ExpressionType!.MakeArrayType();
    }

    public void Visit(CompilationContext context, WhileSyntax node)
    {
        if (node.Condition.ExpressionType != typeof(bool))
        {
            throw new CompilerError($"Expected a boolean expression for while statement condition, got {node.Condition.ExpressionType?.ToString() ?? "null"}");
        }
    }

    public void Visit(CompilationContext context, BinaryExpressionSyntax node)
    {
        node.ExpressionType = node.Operator switch
        {
            BinaryOperator.Add => node.Left.ExpressionType,
            BinaryOperator.Subtract => node.Left.ExpressionType,
            BinaryOperator.Multiply => node.Left.ExpressionType,
            BinaryOperator.Divide => node.Left.ExpressionType,
            BinaryOperator.Modulo => node.Left.ExpressionType,
            BinaryOperator.Equal => typeof(bool),
            BinaryOperator.NotEqual => typeof(bool),
            BinaryOperator.LessThan => typeof(bool),
            BinaryOperator.GreaterThan => typeof(bool),
            BinaryOperator.LessThanOrEqual => typeof(bool),
            BinaryOperator.GreaterThanOrEqual => typeof(bool),
            BinaryOperator.LogicalAnd => typeof(bool),
            BinaryOperator.LogicalOr => typeof(bool),
            BinaryOperator.BitwiseAnd => node.Left.ExpressionType,
            BinaryOperator.BitwiseOr => node.Left.ExpressionType,
            BinaryOperator.BitwiseXor => node.Left.ExpressionType,
            BinaryOperator.BitwiseLeftShift => node.Left.ExpressionType,
            BinaryOperator.BitwiseRightShift => node.Left.ExpressionType,
            BinaryOperator.ArithmeticLeftShift => node.Left.ExpressionType,
            BinaryOperator.ArithmeticRightShift => node.Left.ExpressionType,
            BinaryOperator.Assign => node.Left.ExpressionType,
            BinaryOperator.AddAssign => node.Left.ExpressionType,
            BinaryOperator.SubtractAssign => node.Left.ExpressionType,
            BinaryOperator.MultiplyAssign => node.Left.ExpressionType,
            BinaryOperator.DivideAssign => node.Left.ExpressionType,
            BinaryOperator.ModuloAssign => node.Left.ExpressionType,
            BinaryOperator.BitwiseAndAssign => node.Left.ExpressionType,
            BinaryOperator.BitwiseOrAssign => node.Left.ExpressionType,
            BinaryOperator.BitwiseXorAssign => node.Left.ExpressionType,
            BinaryOperator.BitwiseLeftShiftAssign => node.Left.ExpressionType,
            BinaryOperator.BitwiseRightShiftAssign => node.Left.ExpressionType,
            BinaryOperator.NoneCoalescing => throw new NotImplementedException("Optionals are not yet supported"),
            BinaryOperator.NoneCoalescingAssign => throw new NotImplementedException("Optionals are not yet supported"),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public void Visit(CompilationContext context, UnaryExpressionSyntax node)
    {
        node.ExpressionType = node.Operator switch
        {
            UnaryOperator.PreIncrement => node.Expression.ExpressionType,
            UnaryOperator.PostIncrement => node.Expression.ExpressionType,
            UnaryOperator.PreDecrement => node.Expression.ExpressionType,
            UnaryOperator.PostDecrement => node.Expression.ExpressionType,
            UnaryOperator.Negate => node.Expression.ExpressionType,
            UnaryOperator.Dereference => throw new NotImplementedException("Unsafe code is not yet supported"),
            UnaryOperator.RawAddress => throw new NotImplementedException("Unsafe code is not yet supported"),
            UnaryOperator.LogicalNot => typeof(bool),
            UnaryOperator.BitwiseNot => node.Expression.ExpressionType,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public void Visit(CompilationContext context, TypeCastSyntax node)
    {
        if (node.Fallible)
        {
            throw new NotImplementedException("Optionals are not yet supported");
        }

        node.ExpressionType = node.Type.Type;
    }

    public void Visit(CompilationContext context, TypeCheckSyntax node)
    {
        node.ExpressionType = typeof(bool);
    }
}