namespace Jaktnat.Runtime;

public readonly struct Optional<T>
{
    private readonly T? _value;
    private readonly bool _hasValue;

    public Optional(T value)
    {
        if (value == null)
        {
            throw new InvalidOperationException("Cannot initialize an optional with null");
        }
        
        _value = value;
        _hasValue = true;
    }

    public T Value => _value ?? throw new InvalidOperationException("Optional does not have a value");

    public bool HasValue => _hasValue;

    public static Optional<T> None => new();

    public static Optional<T> Some(T value) => new(value);

    public static implicit operator Optional<T>(T value) => new(value);

    public static explicit operator T(Optional<T> opt) => opt.Value;

    public bool Equals(Optional<T> other) =>
        EqualityComparer<T?>.Default.Equals(_value, other._value) 
        && _hasValue == other._hasValue;

    public override bool Equals(object? obj) => 
        obj is Optional<T> other && Equals(other);

    public override int GetHashCode() => 
        HashCode.Combine(_value, _hasValue);

    public static bool operator ==(Optional<T> left, Optional<T> right) => left.Equals(right);

    public static bool operator !=(Optional<T> left, Optional<T> right) => !(left == right);
}