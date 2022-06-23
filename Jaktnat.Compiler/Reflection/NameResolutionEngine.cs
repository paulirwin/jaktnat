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
    ISyntaxVisitor<TypeCheckSyntax>,
    ISyntaxVisitor<MemberAccessSyntax>,
    ISyntaxVisitor<IndexerAccessSyntax>,
    ISyntaxVisitor<PropertySyntax>,
    ISyntaxVisitor<ClassDeclarationSyntax>
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

        if (varDecl.ParentBlock.Declarations.ContainsKey(varDecl.Name))
        {
            throw new CompilerError($"Variable '{varDecl.Name}' has already been declared in this scope");
        }

        if (varDecl.InitializerExpression != null)
        {
            varDecl.Type = varDecl.InitializerExpression.ExpressionType;
            varDecl.ParentBlock.Declarations.Add(varDecl.Name, varDecl);
        }
        else
        {
            throw new NotImplementedException("Variable declaration without initializer expression");
        }

        if (varDecl.TypeIdentifier != null)
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

        if (identifier.ParentTarget != null)
        {
            if (identifier.ParentTarget.ExpressionType == null)
            {
                throw new CompilerError("Unable to find member on target expression as target hasn't been evaluated properly");
            }

            if (identifier.ParentTarget.ExpressionType is RuntimeTypeReference runtimeType)
            {
                var members = runtimeType.RuntimeType
                    .GetMembers()
                    .Where(i => i.Name == identifier.Name)
                    .ToList();

                if (members.Count == 0)
                {
                    throw new CompilerError($"Unable to resolve identifier {identifier.Name} on type {identifier.ParentTarget.ExpressionType}");
                }

                identifier.ExpressionType = members[0].GetType();
                identifier.Target = members;
            }
            else
            {
                throw new NotImplementedException("Need to support identifier expressions on declared types");
            }
        }
        else if (identifier.ParentBlock.TryResolveDeclaration(identifier.Name, out var varDecl))
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
                case TypeDeclarationSyntax typeDeclaration:
                    identifier.ExpressionType = typeof(Type);
                    identifier.Target = typeDeclaration;
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
        else if (node.Target is IdentifierExpressionSyntax { Target: TypeDeclarationSyntax type })
        {
            node.PossibleMatchedConstructors = type.Constructors;
        }

        ResolveOverloads(node);
    }

    private static void ResolveOverloads(CallSyntax call)
    {
        if (call.PossibleMatchedMethods == null 
            && call.PossibleMatchedConstructors == null)
        {
            throw new InvalidOperationException("Overload resolution called before name resolution");
        }

        var argTypes = call.Arguments.Select(i => i.ArgumentType).ToArray();

        if (call.PossibleMatchedMethods != null)
        {
            if (ResolveMethodOverloads(call, argTypes, out var freeFunction) && freeFunction != null)
            {
                call.MatchedMethod = freeFunction;
                call.ExpressionType = freeFunction.ReturnType;
                return;
            }

            throw new CompilerError($"Unable to resolve overload, ambiguous matches found: {string.Join(", ", call.PossibleMatchedMethods)}");
        }

        if (call.PossibleMatchedConstructors is { Count: > 0 })
        {
            if (ResolveConstructorOverloads(call, argTypes, out var constructor) && constructor != null)
            {
                call.MatchedConstructor = constructor;
                call.ExpressionType = constructor.DeclaringType;
                return;
            }

            throw new CompilerError($"Unable to resolve overload, ambiguous constructor matches found: {string.Join(", ", call.PossibleMatchedConstructors)}");
        }

        throw new CompilerError($"Unable to resolve overload");
    }

    private static bool ResolveConstructorOverloads(CallSyntax call, TypeReference?[] argTypes, out ConstructorSyntax? constructor)
    {
        if (call.PossibleMatchedConstructors == null)
        {
            constructor = null;
            return false;
        }

        // TODO: support matching on parameter name?
        // TODO: support `params` parameters
        // TODO: support overload precedence, i.e. for a String argument, prefer (String s) over (Object o)
        foreach (var possibility in call.PossibleMatchedConstructors)
        {
            if (possibility.Parameters.Parameters.Count == 0
                && call.Arguments.Count == 0)
            {
                constructor = possibility;
                return true;
            }
            else if (call.Arguments.Count > 0)
            {
                var parameterTypes = possibility.Parameters.Parameters
                    .Select(i => i.Type).ToArray();

                if (parameterTypes.Length == call.Arguments.Count
                    && ParameterTypesAreCompatible(parameterTypes, argTypes))
                {
                    constructor = possibility;
                    return true;
                }
            }
            
        }

        constructor = null;
        return false;
    }

    private static bool ResolveMethodOverloads(CallSyntax call, TypeReference?[] argTypes, out FreeFunction? freeFunction)
    {
        if (call.PossibleMatchedMethods == null)
        {
            freeFunction = null;
            return false;
        }

        // TODO: support matching on parameter name?
        // TODO: support `params` parameters
        // TODO: support overload precedence, i.e. for a String argument, prefer (String s) over (Object o)
        foreach (var possibility in call.PossibleMatchedMethods)
        {
            if (possibility is RuntimeMethodInfoFreeFunction runtimeFunction)
            {
                var parameterTypes = runtimeFunction.Method.GetParameters().Select(i => new RuntimeTypeReference(i.ParameterType)).ToArray();

                if (parameterTypes.Length == call.Arguments.Count
                    && ParameterTypesAreCompatible(parameterTypes, argTypes))
                {
                    freeFunction = possibility;
                    return true;
                }
            }
            else if (possibility is DeclaredFreeFunction declaredFunction)
            {
                if (declaredFunction.FunctionSyntax.Parameters == null
                    && call.Arguments.Count == 0)
                {
                    freeFunction = possibility;
                    return true;
                }
                else if (call.Arguments.Count > 0 && declaredFunction.FunctionSyntax.Parameters is { } parameters)
                {
                    var parameterTypes = parameters.Parameters
                        .Select(i => i.Type).ToArray();

                    if (parameterTypes.Length == call.Arguments.Count
                        && ParameterTypesAreCompatible(parameterTypes, argTypes))
                    {
                        freeFunction = possibility;
                        return true;
                    }
                }
            }
        }

        freeFunction = null;
        return false;
    }

    private static bool ParameterTypesAreCompatible(IReadOnlyList<TypeReference?> parameterTypes, IReadOnlyList<TypeReference?> argumentTypes)
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

    internal static bool ParameterTypeIsCompatible(TypeReference parameterType, TypeReference argumentType)
    {
        if (parameterType.Equals(typeof(object)))
        {
            return true;
        }
        
        return parameterType is DeclaredTypeReference declaredType && argumentType is DeclaredTypeReference argumentDeclaredType
            ?  declaredType.DeclaredType == argumentDeclaredType.DeclaredType // TODO: support type inheritance / hierarchy
            : parameterType is RuntimeTypeReference { RuntimeType: Type runtimeType } && argumentType is RuntimeTypeReference { RuntimeType: Type argumentRuntimeType } && runtimeType.IsAssignableFrom(argumentRuntimeType);
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
        if (context.CompilationUnit.TryResolveType(node.Name, out var typeDecl) && typeDecl != null)
        {
            node.Type = typeDecl;
        }
        else if (BuiltInTypeResolver.TryResolveType(node.Name) is Type type)
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
        if (node.ParentBlock == null)
        {
            throw new CompilerError("Expected parameter to have a block");
        }

        if (node.TypeIdentifier == null)
        {
            throw new CompilerError("Expected type identifier in parameter declaration");
        }

        node.ParentBlock.Declarations.Add(node.Name, node);
        node.Type = node.TypeIdentifier.Type;
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
        if (node.Condition.ExpressionType is null || !node.Condition.ExpressionType.Equals(typeof(bool)))
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

    public void Visit(CompilationContext context, MemberAccessSyntax node)
    {
        if (node.Member.Target is not IList<MemberInfo> members)
        {
            throw new CompilerError("Member access expression has not been evaluated yet");
        }

        if (members.Count == 0)
        {
            throw new CompilerError("Member access expression has no members");
        }

        if (members.Count == 1)
        {
            if (members[0] is FieldInfo field)
            {
                node.ExpressionType = field.FieldType;
                node.Member.Target = field;
            }
            else if (members[0] is PropertyInfo property)
            {
                node.ExpressionType = property.PropertyType;
                node.Member.Target = property;
            }
            else if (members[0] is MethodInfo methodInfo)
            {
                node.ExpressionType = typeof(MethodInfo);
            }
        }
        else
        {
            if (!members.All(i => i is MethodInfo))
            {
                throw new CompilerError($"Expected method overloads, but found other types that match member {node.Member.Name}");
            }

            node.ExpressionType = typeof(MethodInfo);
        }
    }

    public void Visit(CompilationContext context, IndexerAccessSyntax node)
    {
        if (node.Target.ExpressionType == null)
        {
            throw new CompilerError("Indexer access target has not been evaluated yet");
        }
        
        // TODO: support custom indexers
        if (!node.Target.ExpressionType.IsArray)
        {
            throw new CompilerError("Indexer access is currently only supported on array types");
        }

        node.ExpressionType = node.Target.ExpressionType.GetElementType();
    }

    public void Visit(CompilationContext context, PropertySyntax node)
    {
        node.Type = node.TypeIdentifier.Type;
    }

    public void Visit(CompilationContext context, ClassDeclarationSyntax node)
    {
        context.CompilationUnit.DeclareType(node.Name, node);

        var parameters = node.Members.OfType<PropertySyntax>()
            .Select(i => new ParameterSyntax(false, i.Name, false, i.TypeIdentifier) { ParentBlock = node.ParentBlock, Type = i.Type })
            .ToList();

        node.Constructors.Add(new ConstructorSyntax(node, new ParameterListSyntax(parameters)));
    }
}