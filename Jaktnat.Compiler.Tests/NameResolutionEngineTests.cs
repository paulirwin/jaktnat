using Jaktnat.Compiler.Reflection;

namespace Jaktnat.Compiler.Tests;

public class NameResolutionEngineTests
{
    [InlineData(typeof(string), typeof(string), true)]
    [InlineData(typeof(object), typeof(string), true)]
    [InlineData(typeof(string), typeof(int), false)]
    [InlineData(typeof(long), typeof(int), true)]
    [Theory]
    public void ParameterTypeIsCompatibleTests(Type parameterType, Type argumentType, bool isCompatible)
    {
        var result = NameResolutionEngine.ParameterTypeIsCompatible(parameterType, argumentType, false);

        Assert.Equal(isCompatible, result);
    }
}