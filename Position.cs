// ReSharper disable NonReadonlyMemberInGetHashCode
namespace othello;

public class Position(int x, int y)
{
    
    public int X {get; set;} = x;
    public int Y  {get; set;} = y;


    public bool Equals(Position other)
    {
        return X == other.X && Y == other.Y;
    }

    public override int GetHashCode() =>
        HashCode.Combine(X, Y);
    public Position Clone()
    {
        return new Position(X, Y);
    }

    public override string ToString()
    {
        return $"({X},{Y})";
    }
    
}