using Jaktnat.Compiler.Syntax;
using Mono.Cecil.Cil;

namespace Jaktnat.Compiler.ILGenerators;

internal static class ExpressionILGenerator
{
    public static void GenerateExpression(CompilationContext context, ILProcessor il, ExpressionSyntax expression, Type? targetType)
    {
        if (expression is LiteralExpressionSyntax literal)
        {
            if (targetType == null || literal.ExpressionType == targetType)
            {
                var op = GetLoadLiteralOp(il, literal.Value);

                il.Append(op);
            }
            else if (literal.ExpressionType is { IsValueType: true } && targetType == typeof(object))
            {
                var tempType = il.Body.Method.DeclaringType.Module.ImportReference(literal.ExpressionType);

                il.Append(GetLoadLiteralOp(il, literal.Value)); // put literal on stack
                il.Append(il.Create(OpCodes.Box, tempType)); // constrain (box) value type to parameter type
            }
        }
        else if (expression is IdentifierExpressionSyntax identifier)
        {
            if (identifier.ParentBlock == null)
            {
                throw new CompilerError("Identifier used outside of a block scope");
            }

            if (!identifier.ParentBlock.TryResolveDeclaration(identifier.Name, out var decl))
            {
                throw new CompilerError($"Identifier '{identifier.Name}' could not be resolved");
            }

            if (decl is VariableDeclarationSyntax varDecl)
            {
                if (varDecl.ILVariableDefinition == null || varDecl.Type == null)
                {
                    throw new CompilerError($"Attempt to use variable '{varDecl.Name}' before it has been declared");
                }

                il.Append(il.Create(OpCodes.Ldloc_S, varDecl.ILVariableDefinition));

                if (varDecl.Type.IsValueType && targetType == typeof(object))
                {
                    il.Append(il.Create(OpCodes.Box, varDecl.ILVariableDefinition.VariableType)); // constrain (box) value type to parameter type
                }
            }
            else
            {
                throw new NotImplementedException($"Declaration type {decl.GetType()} is not yet implemented");
            }
        }
        else if (expression is CallSyntax call)
        {
            FunctionCallILGenerator.GenerateFunctionCall(context, il, call);
        }
        else if (expression is ArraySyntax array)
        {
            if (array.ExpressionType?.GetElementType() is not Type elementType)
            {
                throw new CompilerError("Array initializer does not have a type or is not an array");
            }
            
            var elementTypeRef = il.Body.Method.DeclaringType.Module.ImportReference(elementType);

            il.Append(il.Create(OpCodes.Ldc_I4, array.ItemsList.Items.Count));
            il.Append(il.Create(OpCodes.Newarr, elementTypeRef));

            int index = 0;
            foreach (var item in array.ItemsList.Items)
            {
                il.Append(il.Create(OpCodes.Dup)); // copy arr on stack
                il.Append(il.Create(OpCodes.Ldc_I4, index)); // push index on stack
                GenerateExpression(context, il, item, elementType); // push value on stack
                il.Append(il.Create(GetStoreElementOpcode(elementType))); // arr[index] = item

                index++;
            }
        }
        else if (expression is BinaryExpressionSyntax binaryExpression)
        {
            if (binaryExpression.Operator is BinaryOperator.LessThan or BinaryOperator.LessThanOrEqual or BinaryOperator.GreaterThan or BinaryOperator.GreaterThanOrEqual or BinaryOperator.Equal or BinaryOperator.NotEqual)
            {
                GenerateExpression(context, il, binaryExpression.Left, binaryExpression.Left.ExpressionType);
                GenerateExpression(context, il, binaryExpression.Right, binaryExpression.Right.ExpressionType);

                il.Append(il.Create(GetBooleanBinaryOpcode(binaryExpression.Operator)));
            }
            else
            {
                throw new NotImplementedException($"Binary expression operator {binaryExpression.Operator} is not yet implemented");
            }

        }
        else
        {
            throw new NotImplementedException($"Expression type {expression.GetType()} is not yet implemented");
        }
    }

    private static OpCode GetBooleanBinaryOpcode(BinaryOperator op)
    {
        return op switch
        {
            BinaryOperator.LessThan => OpCodes.Clt,
            BinaryOperator.GreaterThan => OpCodes.Cgt,
            _ => throw new NotImplementedException($"Boolean operation {op} is not yet implemented")
        };
    }

    private static OpCode GetStoreElementOpcode(Type elementType)
    {
        if (elementType == typeof(byte)) // TODO: is this correct for Stelem_I1?
        {
            return OpCodes.Stelem_I1;
        }
        if (elementType == typeof(short))
        {
            return OpCodes.Stelem_I2;
        }
        if (elementType == typeof(int))
        {
            return OpCodes.Stelem_I4;
        }
        if (elementType == typeof(long))
        {
            return OpCodes.Stelem_I8;
        }
        if (elementType == typeof(float))
        {
            return OpCodes.Stelem_R4;
        }
        if (elementType == typeof(double))
        {
            return OpCodes.Stelem_R8;
        }

        throw new NotImplementedException($"Initializing an array of type {elementType} is not yet implemented");
    }

    private static Instruction GetLoadLiteralOp(ILProcessor il, object value)
    {
        var op = value switch
        {
            string str => il.Create(OpCodes.Ldstr, str),
            long l => il.Create(OpCodes.Ldc_I8, l),
            double d => il.Create(OpCodes.Ldc_R8, d),
            true => il.Create(OpCodes.Ldc_I4_1),
            false => il.Create(OpCodes.Ldc_I4_0),
            _ => throw new NotImplementedException($"Passing a {value.GetType()} literal as a function argument is not yet implemented"),
        };
        return op;
    }
}