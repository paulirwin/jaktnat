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
                var op = GetLoadLiteralOp(il, literal);

                il.Append(op);
            }
            else if (literal.ExpressionType is { IsValueType: true } && targetType == typeof(object))
            {
                var tempType = il.Body.Method.DeclaringType.Module.ImportReference(literal.ExpressionType);

                il.Append(GetLoadLiteralOp(il, literal)); // put literal on stack
                il.Append(il.Create(OpCodes.Box, tempType)); // constrain (box) value type to parameter type
            }
        }
        else if (expression is IdentifierExpressionSyntax identifier)
        {
            if (identifier.ParentBlock == null)
            {
                throw new CompilerError("Identifier used outside of a block scope");
            }

            if (!identifier.ParentBlock.TryResolveVariable(identifier.Name, out var varDecl))
            {
                throw new CompilerError($"Identifier '{identifier.Name}' could not be resolved");
            }

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
        else if (expression is CallSyntax call)
        {
            FunctionCallILGenerator.GenerateFunctionCall(context, il, call);
        }
        else
        {
            throw new NotImplementedException($"Expression type {expression.GetType()} is not yet implemented");
        }
    }

    private static Instruction GetLoadLiteralOp(ILProcessor il, LiteralExpressionSyntax literal)
    {
        var op = literal.Value switch
        {
            string str => il.Create(OpCodes.Ldstr, str),
            long l => il.Create(OpCodes.Ldc_I8, l),
            double d => il.Create(OpCodes.Ldc_R8, d),
            true => il.Create(OpCodes.Ldc_I4_1),
            false => il.Create(OpCodes.Ldc_I4_0),
            _ => throw new NotImplementedException($"Passing a {literal.Value.GetType()} literal as a function argument is not yet implemented"),
        };
        return op;
    }
}