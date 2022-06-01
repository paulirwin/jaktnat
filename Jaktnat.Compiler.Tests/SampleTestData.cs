using System.Collections;

namespace Jaktnat.Compiler.Tests;

public class SampleTestData : IEnumerable<object?[]>
{
    public IEnumerator<object?[]> GetEnumerator()
    {
        var assembly = typeof(SampleTestData).Assembly;

        return assembly.GetManifestResourceNames()
            .Where(i => i.EndsWith(".jakt"))
            .Select(i =>
            {
                return new object?[] { i };
            })
            .GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}