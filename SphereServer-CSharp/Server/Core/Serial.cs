namespace SphereServer.Core;

/// <summary>
/// Represents a unique identifier for game objects (items and characters).
/// Based on Source-X CUID system.
/// </summary>
public readonly struct Serial : IEquatable<Serial>, IComparable<Serial>
{
    public const uint MinusOne = 0xFFFFFFFF;
    public const uint Zero = 0;
    public const uint World = 1; // First valid serial
    public const uint ItemMin = 0x40000000; // Items start at this serial
    public const uint CharMin = 0x00000001; // Characters start at 1
    public const uint CharMax = 0x3FFFFFFF; // Characters end before items

    private readonly uint _value;

    public Serial(uint value)
    {
        _value = value;
    }

    public uint Value => _value;

    public bool IsValid => _value > Zero && _value < MinusOne;
    public bool IsItem => _value >= ItemMin && _value < MinusOne;
    public bool IsChar => _value >= CharMin && _value <= CharMax;
    public bool IsZero => _value == Zero;

    public static Serial Invalid => new(MinusOne);
    public static Serial WorldSerial => new(World);

    public bool Equals(Serial other) => _value == other._value;
    public override bool Equals(object? obj) => obj is Serial serial && Equals(serial);
    public override int GetHashCode() => _value.GetHashCode();
    public int CompareTo(Serial other) => _value.CompareTo(other._value);

    public static bool operator ==(Serial left, Serial right) => left.Equals(right);
    public static bool operator !=(Serial left, Serial right) => !left.Equals(right);
    public static bool operator <(Serial left, Serial right) => left._value < right._value;
    public static bool operator >(Serial left, Serial right) => left._value > right._value;
    public static bool operator <=(Serial left, Serial right) => left._value <= right._value;
    public static bool operator >=(Serial left, Serial right) => left._value >= right._value;

    public static implicit operator uint(Serial serial) => serial._value;
    public static implicit operator Serial(uint value) => new(value);

    public override string ToString() => $"0x{_value:X8}";
}
