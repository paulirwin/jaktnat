namespace Jaktnat.Runtime;

public static class ConsoleFunctions
{
    [FreeFunction("println")]
    public static void Println(string format)
    {
        Console.WriteLine(format);
    }
}