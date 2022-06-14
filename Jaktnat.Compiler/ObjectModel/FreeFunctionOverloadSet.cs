namespace Jaktnat.Compiler.ObjectModel;

internal class FreeFunctionOverloadSet
{
    public IList<FreeFunction> FreeFunctions { get; } = new List<FreeFunction>();

    public static FreeFunctionOverloadSet FromPair(FreeFunction first, FreeFunction second)
    {
        return new FreeFunctionOverloadSet
        {
            FreeFunctions =
            {
                first,
                second,
            }
        };
    }
}