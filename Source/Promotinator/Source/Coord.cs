namespace Promotinator.Engine;

public struct Coord : IEquatable<Coord> {
    public static Coord None = new(-1, -1);

    public readonly int File;
    public readonly int Rank;

    public Coord(int file, int rank) {
        File = file;
        Rank = rank;
    }

    public string ToAlgabraicNotation() {
        char from = (char)(File + 'a');
        int to = Rank + 1;
        return $"{from}{to}";
    }

    public override string ToString() {
        return $"{{File:{File} Rank:{Rank}}}";
    }

    public override bool Equals(object? obj) {
        return obj is Coord && Equals(obj);
    }

    public override int GetHashCode() {
        return File.GetHashCode() ^ Rank.GetHashCode();
    }

    public bool Equals(Coord other) {
        return File == other.File && Rank == other.Rank;
    }

    public static bool operator ==(Coord left, Coord right) {
        return left.Equals(right);
    }

    public static bool operator !=(Coord left, Coord right) {
        return !(left == right);
    }
}
