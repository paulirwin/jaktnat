namespace Jaktnat.Runtime;

public sealed class Defer : IDisposable
{
    private readonly Action _onDispose;
    
    public Defer(Action onDispose) => _onDispose = onDispose;

    public void Dispose() => _onDispose();

    public override string ToString() => _onDispose.ToString() ?? nameof(Defer);

    private bool Equals(Defer other) => _onDispose.Equals(other._onDispose);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) 
                                                || obj is Defer other 
                                                && Equals(other);

    public override int GetHashCode() => _onDispose.GetHashCode();
}