namespace Jaktnat.Compiler;

internal static class ArrayExtensions
{
    public static int IndexOf<T>(this IReadOnlyList<T?> list, T? item)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if ((item == null && list[i] == null)
                || (item != null && item.Equals(list[i])))
            {
                return i;
            }
        }

        return -1;
    }
}