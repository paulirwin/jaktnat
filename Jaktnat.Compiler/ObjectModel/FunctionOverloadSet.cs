namespace Jaktnat.Compiler.ObjectModel;

internal class FunctionOverloadSet
{
    public IList<Function> Functions { get; } = new List<Function>();

    public static FunctionOverloadSet FromPair(Function first, Function second)
    {
        return new FunctionOverloadSet
        {
            Functions =
            {
                first,
                second,
            }
        };
    }
}