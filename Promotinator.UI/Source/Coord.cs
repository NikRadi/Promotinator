using System;

namespace Promotinator.UI;

public struct Coord : IEquatable<Coord> {
    public readonly int File;
    public readonly int Rank;

    public Coord(int file , int rank) {
        if (file < 0 || file >= 8 || rank < 0 || rank >= 8) {
            throw new ArgumentException($"Invalid file/rank: file:{file} rank:{rank}");
        }

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

    public override bool Equals(object obj) {
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
