namespace SphereServer.Core;

/// <summary>
/// Represents a point in 3D space with map information.
/// Based on Source-X CPointBase/CPointMap.
/// </summary>
public struct Point3D : IEquatable<Point3D>
{
    public short X { get; set; }
    public short Y { get; set; }
    public sbyte Z { get; set; }
    public byte Map { get; set; }

    public Point3D(short x, short y, sbyte z = 0, byte map = 0)
    {
        X = x;
        Y = y;
        Z = z;
        Map = map;
    }

    public static Point3D Invalid => new(-1, -1, 0, 0);
    public static Point3D Zero => new(0, 0, 0, 0);

    public bool IsValidXY => X >= 0 && Y >= 0;
    public bool IsValid => IsValidXY && Map < 256;

    public int GetDistance(Point3D other)
    {
        int dx = Math.Abs(X - other.X);
        int dy = Math.Abs(Y - other.Y);
        return Math.Max(dx, dy);
    }

    public int GetDistance3D(Point3D other)
    {
        int dx = X - other.X;
        int dy = Y - other.Y;
        int dz = Z - other.Z;
        return (int)Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    public bool IsSame2D(Point3D other)
    {
        return X == other.X && Y == other.Y && Map == other.Map;
    }

    public Direction GetDirection(Point3D target)
    {
        int dx = target.X - X;
        int dy = target.Y - Y;

        if (dx == 0 && dy == 0)
            return Direction.North;

        double angle = Math.Atan2(dy, dx) * 180.0 / Math.PI;

        if (angle < 0)
            angle += 360;

        return (Direction)(((int)(angle + 22.5) / 45) % 8);
    }

    public void Move(Direction dir, int amount = 1)
    {
        for (int i = 0; i < amount; i++)
        {
            switch (dir)
            {
                case Direction.North:
                    Y--;
                    break;
                case Direction.NorthEast:
                    Y--;
                    X++;
                    break;
                case Direction.East:
                    X++;
                    break;
                case Direction.SouthEast:
                    Y++;
                    X++;
                    break;
                case Direction.South:
                    Y++;
                    break;
                case Direction.SouthWest:
                    Y++;
                    X--;
                    break;
                case Direction.West:
                    X--;
                    break;
                case Direction.NorthWest:
                    Y--;
                    X--;
                    break;
            }
        }
    }

    public bool Equals(Point3D other)
    {
        return X == other.X && Y == other.Y && Z == other.Z && Map == other.Map;
    }

    public override bool Equals(object? obj) => obj is Point3D point && Equals(point);
    public override int GetHashCode() => HashCode.Combine(X, Y, Z, Map);

    public static bool operator ==(Point3D left, Point3D right) => left.Equals(right);
    public static bool operator !=(Point3D left, Point3D right) => !left.Equals(right);

    public override string ToString() => $"({X}, {Y}, {Z}) [Map: {Map}]";
}

/// <summary>
/// Direction enumeration for movement.
/// </summary>
public enum Direction : byte
{
    North = 0,
    NorthEast = 1,
    East = 2,
    SouthEast = 3,
    South = 4,
    SouthWest = 5,
    West = 6,
    NorthWest = 7,
    Invalid = 8
}
