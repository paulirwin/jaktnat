namespace Jaktnat.Runtime;

public class Error : Exception
{
    public Error(int code)
    {
        HResult = code;
    }

    public static Error from_errno(int code) => new(code);
}