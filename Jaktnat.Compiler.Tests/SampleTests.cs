using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jaktnat.Compiler.Tests;

public class SampleTests
{
    [Theory]
    [ClassData(typeof(SampleTestData))]
    public void SampleTestRunner(string filePath)
    {
        using var stream = typeof(SampleTests).Assembly.GetManifestResourceStream(filePath);

        if (stream == null)
        {
            throw new InvalidOperationException($"Could not open stream for embedded resource at path {filePath}");
        }

        using var reader = new StreamReader(stream);
        var contents = reader.ReadToEnd();

        var expectation = ParseExpectation(contents);

        var assemblyName = Path.GetFileNameWithoutExtension(filePath);
        Assembly assembly;

        var options = new BuildOptions
        {
            Backend = BackendType.Roslyn,
        };

        try
        {
            assembly = JaktnatCompiler.CompileText(options, assemblyName, contents);
        }
        catch
        {
            if (expectation.Error == null)
            {
                throw;
            }

            return;
        }

        var programType = assembly.GetType($"{assemblyName}.Program");

        if (programType == null)
        {
            throw new InvalidOperationException("Jaktnat compiler output assembly does not contain a Program class");
        }
        
        var mainMethod = programType.GetMethod("Main", BindingFlags.Static | BindingFlags.Public);

        if (mainMethod == null)
        {
            throw new InvalidOperationException("Jaktnat compiler output's Program class does not have a Main method");
        }

        using var sw = new StringWriter();

        Console.SetOut(sw);

        try
        {
            mainMethod.Invoke(null, new object[] { Array.Empty<string>() });
        }
        catch
        {
            if (expectation.Error == null)
            {
                throw;
            }

            return;
        }

        var output = sw.ToString().ReplaceLineEndings("\n");

        if (expectation.Output != null)
        {
            Assert.Equal(expectation.Output, output);
        }
        else if (expectation.Error != null)
        {
            throw new InvalidOperationException("Expected an error, but app ran successfully");
        }
    }

    private static Expectation ParseExpectation(string contents)
    {
        using var sr = new StringReader(contents);
        var expectation = new Expectation();

        while (sr.ReadLine() is { } line && line.StartsWith("///"))
        {
            line = line.TrimStart('/', ' ');

            if (!line.StartsWith("-"))
            {
                continue;
            }

            line = line.TrimStart('-', ' ');

            if (line.StartsWith("output: "))
            {
                line = line["output: ".Length..];

                // HACK.PI: use C# string escape format for output string
                var syntax = CSharpSyntaxTree.ParseText(line);

                var root = syntax.GetRoot();

                var literal = FindLiteralExpressionSyntax(root);

                if (literal != null) 
                {
                    expectation.Output = literal.Token.ValueText;
                }
                else
                {
                    throw new InvalidOperationException("Unable to parse output expectation as a string");
                }
            }
            else if (line.StartsWith("error: "))
            {
                line = line["error: ".Length..];

                expectation.Error = line; // TODO: validate error message
            }
        }

        return expectation;
    }

    private static LiteralExpressionSyntax? FindLiteralExpressionSyntax(SyntaxNode node)
    {
        if (node is LiteralExpressionSyntax literal)
        {
            return literal;
        }

        return node.ChildNodes()
            .Select(FindLiteralExpressionSyntax)
            .FirstOrDefault(childLiteral => childLiteral != null);
    }

}