namespace Jaktnat.Compiler.ObjectModel;

internal class FunctionOverloadSet
{
    public FunctionOverloadSet()
    {
    }

    public FunctionOverloadSet(IEnumerable<Function> functions)
    {
        Functions = functions.ToList();
    }
    
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