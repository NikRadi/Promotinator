namespace Promotinator.Engine;

public struct Coord {
    public static Coord None = new(-1, -1);

    public readonly int File;
    public readonly int Rank;

    public Coord(int file, int rank) {
        File = file;
        Rank = rank;
    }

    public Coord(string square) {
        File = square[0] - 'a';
        Rank = square[1] - '1';
    }

    public string ToAlgabraicNotation() {
        char from = (char) (File + 'a');
        int to = Rank + 1;
        return $"{from}{to}";
    }

    public override string ToString() {
        return $"{{File:{File} Rank:{Rank}}}";
    }
}
