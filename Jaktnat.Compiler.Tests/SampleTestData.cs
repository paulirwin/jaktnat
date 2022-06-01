using System.Collections;

namespace Jaktnat.Compiler.Tests;

public class SampleTestData : IEnumerable<object?[]>
{
    public IEnumerator<object?[]> GetEnumerator()
    {
        var assembly = typeof(SampleTestData).Assembly;
        var ns = typeof(SampleTestData).Namespace ?? "";

        return assembly.GetManifestResourceNames()
            .Where(i => i.EndsWith(".jakt"))
            .Select(i =>
            {
                using var stream = assembly.GetManifestResourceStream(i);

                var filePath = i.StartsWith(ns) ? i[(ns.Length + 1)..] : i;

                if (stream == null)
                {
                    return new object?[] { filePath, null };
                }

                using var reader = new StreamReader(stream);

                return new object?[] { filePath, reader.ReadToEnd() };
            })
            .Where(i => i[1] != null)
            .GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}