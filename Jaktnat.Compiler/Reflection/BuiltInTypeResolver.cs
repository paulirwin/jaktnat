namespace Jaktnat.Compiler.Reflection;

public static class BuiltInTypeResolver
{
    private static readonly IDictionary<string, Type> _builtInTypes = new Dictionary<string, Type>()
    {
        ["void"] = typeof(void),
        ["bool"] = typeof(bool),
        ["f32"] = typeof(float),
        ["f64"] = typeof(double),
        ["u8"] = typeof(byte),
        ["u16"] = typeof(ushort),
        ["u32"] = typeof(uint),
        ["u64"] = typeof(ulong),
        ["uz"] = typeof(nuint),
        ["i8"] = typeof(sbyte),
        ["i16"] = typeof(short),
        ["i32"] = typeof(int),
        ["i64"] = typeof(long),
        ["String"] = typeof(string),
    };

    public static Type? TryResolveType(string name)
    {
        return _builtInTypes.TryGetValue(name, out var type) ? type : null;
    }
}