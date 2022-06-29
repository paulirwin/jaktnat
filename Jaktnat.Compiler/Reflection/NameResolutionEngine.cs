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
    ISyntaxVisitor<ClassDeclarationSyntax>,
    ISyntaxVisitor<ParenthesizedExpressionSyntax>,
    ISyntaxVisitor<ScopeAccessSyntax>,
    ISyntaxVisitor<CatchIdentifierSyntax>
{
    private static readonly IReadOnlyList<Type> _signedWideningTypes = new[]
    {
        typeof(sbyte),
        typeof(short),
        typeof(int),
        typeof(long),
    };

    private static readonly IReadOnlyList<Type> _unsignedWideningTypes = new[]
    {
        typeof(byte),
        typeof(ushort),
        typeof(uint),
        typeof(ulong),
    };

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
            varDecl.Type = varDecl.TypeIdentifier.Type;
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
            else if (identifier.ParentTarget.ExpressionType is DeclaredTypeReference { DeclaredType: ClassDeclarationSyntax classDecl })
            {
                var member = classDecl.Members.FirstOrDefault(i => i.Name == identifier.Name);

                if (member == null)
                {
                    throw new CompilerError($"Unable to resolve identifier {identifier.Name} on type {identifier.ParentTarget.ExpressionType}");
                }

                if (member is not PropertySyntax property)
                {
                    throw new NotImplementedException("Support for non-properties being used as identifiers is not yet implemented");
                }

                identifier.ExpressionType = property.Type;
                identifier.Target = property;
            }
            else
            {
                throw new CompilerError("Unknown type reference type");
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
                case Function function:
                    identifier.ExpressionType = typeof(MethodInfo);
                    identifier.Target = function;
                    break;
                case FunctionOverloadSet overloadSet:
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
        else if (BuiltInTypeResolver.TryResolveType(identifier.Name) is Type builtInType)
        {
            identifier.ExpressionType = builtInType;
            identifier.Target = builtInType;
        }
        else if (RuntimeTypeResolver.TryResolveType(context, identifier.Name) is Type runtimeType)
        {
            identifier.ExpressionType = runtimeType;
            identifier.Target = runtimeType;
        }
        else
        {
            throw new CompilerError($"Unable to resolve identifier '{identifier.Name}'");
        }
    }

    public void Visit(CompilationContext context, CallSyntax node)
    {
        if (node.Target is IdentifierExpressionSyntax { Target: Function function })
        {
            node.PossibleMatchedMethods = new List<Function> { function };
        }
        else if (node.Target is IdentifierExpressionSyntax { Target: FunctionOverloadSet overloadSet })
        {
            node.PossibleMatchedMethods = overloadSet.Functions;
        }
        else if (node.Target is IdentifierExpressionSyntax { Target: TypeDeclarationSyntax type })
        {
            node.PossibleMatchedConstructors = type.Constructors;
        }
        else if (node.Target is ScopeAccessSyntax { Member: { Target: MethodInfo method} })
        {
            node.PossibleMatchedMethods = new List<Function> { new RuntimeMethodInfoFunction(method) };
        }
        else if (node.Target is ScopeAccessSyntax { Member: { Target: IList<MemberInfo> members } })
        {
            if (!members.All(i => i is MethodInfo))
            {
                throw new CompilerError("Cannot call non-method members");
            }

            node.PossibleMatchedMethods = members
                .Cast<MethodInfo>()
                .Select(i => new RuntimeMethodInfoFunction(i))
                .Cast<Function>()
                .ToList();
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

        if (call.PossibleMatchedMethods is { Count: > 0 })
        {
            if ((ResolveMethodOverloads(call, argTypes, false, out var freeFunction) 
                 || ResolveMethodOverloads(call, argTypes, true, out freeFunction)) 
                && freeFunction != null)
            {
                call.MatchedMethod = freeFunction;
                call.ExpressionType = freeFunction.ReturnType;
                return;
            }

            if (call.PossibleMatchedMethods.Count > 1)
            {
                throw new CompilerError($"Unable to resolve overload, ambiguous matches found: {string.Join(", ", call.PossibleMatchedMethods)}");
            }
            else
            {
                throw new CompilerError($"Unable to resolve function call, argument types do not match: {call.PossibleMatchedMethods[0]}");
            }
        }

        if (call.PossibleMatchedConstructors is { Count: > 0 })
        {
            if ((ResolveConstructorOverloads(call, argTypes, false, out var constructor) 
                 || ResolveConstructorOverloads(call, argTypes, true, out constructor))
                && constructor != null)
            {
                call.MatchedConstructor = constructor;
                call.ExpressionType = constructor.DeclaringType;
                return;
            }

            if (call.PossibleMatchedConstructors.Count > 1)
            {
                throw new CompilerError($"Unable to resolve overload, ambiguous constructor matches found: {string.Join(", ", call.PossibleMatchedConstructors)}");
            }
            else
            {
                throw new CompilerError($"Unable to resolve constructor call, argument types do not match: {call.PossibleMatchedConstructors[0]}");
            }
        }

        throw new CompilerError($"Unable to resolve overload");
    }

    private static bool ResolveConstructorOverloads(CallSyntax call, TypeReference?[] argTypes, bool allowImplicitLiteralConversion, out ConstructorSyntax? constructor)
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
                    && ParameterTypesAreCompatible(parameterTypes, argTypes, allowImplicitLiteralConversion))
                {
                    constructor = possibility;

                    return true;
                }
            }
            
        }

        constructor = null;
        return false;
    }

    private static bool ResolveMethodOverloads(CallSyntax call, TypeReference?[] argTypes, bool allowImplicitLiteralConversion, out Function? freeFunction)
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
            if (possibility is RuntimeMethodInfoFunction runtimeFunction)
            {
                var parameterTypes = runtimeFunction.Method.GetParameters().Select(i => new RuntimeTypeReference(i.ParameterType)).ToArray();

                if (parameterTypes.Length == call.Arguments.Count
                    && ParameterTypesAreCompatible(parameterTypes, argTypes, allowImplicitLiteralConversion))
                {
                    freeFunction = possibility;
                    return true;
                }
            }
            else if (possibility is DeclaredFunction declaredFunction)
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
                        && ParameterTypesAreCompatible(parameterTypes, argTypes, allowImplicitLiteralConversion))
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

    private static bool ParameterTypesAreCompatible(IReadOnlyList<TypeReference?> parameterTypes, IReadOnlyList<TypeReference?> argumentTypes, bool allowImplicitLiteralConversion)
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

            if (argumentType == null 
                || parameterType == null 
                || !ParameterTypeIsCompatible(parameterType, argumentType, allowImplicitLiteralConversion))
            {
                return false;
            }
        }

        return true;
    }

    internal static bool ParameterTypeIsCompatible(TypeReference parameterType, TypeReference argumentType, bool allowImplicitLiteralConversion)
    {
        if (parameterType.Equals(typeof(object)))
        {
            return true;
        }

        if (parameterType is DeclaredTypeReference declaredType 
            && argumentType is DeclaredTypeReference argumentDeclaredType)
        {
            return declaredType.DeclaredType == argumentDeclaredType.DeclaredType; // TODO: support type inheritance / hierarchy
        }
        
        return parameterType is RuntimeTypeReference { RuntimeType: Type parameterRuntimeType } 
               && argumentType is RuntimeTypeReference { RuntimeType: Type argumentRuntimeType } 
               && (parameterRuntimeType.IsAssignableFrom(argumentRuntimeType)
               || PrimitivesAreImplicitlyConvertible(parameterRuntimeType, argumentRuntimeType)
               || (allowImplicitLiteralConversion && LiteralsAreImplicitlyConvertible(parameterRuntimeType, argumentRuntimeType)));
    }

    private static bool LiteralsAreImplicitlyConvertible(Type parameterType, Type argumentType)
    {
        if (!parameterType.IsPrimitive || !argumentType.IsPrimitive)
        {
            return false;
        }

        int paramIndex = _signedWideningTypes.IndexOf(parameterType);
        int argIndex = _signedWideningTypes.IndexOf(argumentType);

        if (paramIndex >= 0 && argIndex >= 0)
        {
            // HACK: we're letting the C# compiler decide if this is valid
            return paramIndex <= argIndex;
        }

        paramIndex = _unsignedWideningTypes.IndexOf(parameterType);
        argIndex = _unsignedWideningTypes.IndexOf(argumentType);

        // HACK: we're letting the C# compiler decide if this is valid
        return paramIndex >= 0 && argIndex >= 0 && paramIndex <= argIndex;
    }

    private static bool PrimitivesAreImplicitlyConvertible(Type parameterType, Type argumentType)
    {
        if (!parameterType.IsPrimitive || !argumentType.IsPrimitive)
        {
            return false;
        }

        int paramIndex = _signedWideningTypes.IndexOf(parameterType);
        int argIndex = _signedWideningTypes.IndexOf(argumentType);

        if (paramIndex >= 0 && argIndex >= 0)
        {
            return paramIndex >= argIndex;
        }

        paramIndex = _unsignedWideningTypes.IndexOf(parameterType);
        argIndex = _unsignedWideningTypes.IndexOf(argumentType);

        return paramIndex >= 0 && argIndex >= 0 && paramIndex >= argIndex;
    }

    public void Visit(CompilationContext context, FunctionSyntax node)
    {
        node.ReturnType = node.ReturnTypeIdentifier != null ? node.ReturnTypeIdentifier.Type : typeof(void);

        context.CompilationUnit.DeclareFreeFunction(node.Name, new DeclaredFunction(node));
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
        if (node.Member.Target is IList<MemberInfo> members)
        {
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
                else if (members[0] is MethodInfo)
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
        else if (node.Member.Target is PropertySyntax property)
        {
            node.ExpressionType = property.Type;
        }
        else
        {
            throw new CompilerError("Member access expression has not been evaluated yet");
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

    public void Visit(CompilationContext context, ParenthesizedExpressionSyntax node)
    {
        node.ExpressionType = node.Expression.ExpressionType;
    }

    public void Visit(CompilationContext context, ScopeAccessSyntax node)
    {
        if (node.Member.Target is IList<MemberInfo> members)
        {
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
                else if (members[0] is MethodInfo)
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
        else if (node.Member.Target is PropertySyntax property)
        {
            node.ExpressionType = property.Type;
        }
        else
        {
            throw new CompilerError("Member access expression has not been evaluated yet");
        }
    }

    public void Visit(CompilationContext context, CatchIdentifierSyntax node)
    {
        if (node.ParentBlock == null)
        {
            throw new CompilerError("Scope resolution for catch block not done properly, should always have a parent block");
        }

        node.ParentBlock.Declarations.Add(node.Name, node);
        node.Type = typeof(Exception);
    }
}