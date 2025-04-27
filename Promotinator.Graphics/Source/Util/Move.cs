using System;

namespace Promotinator.Graphics.Util;

public struct Move :IEquatable<Move> {
    public readonly Coord From;
    public readonly Coord To;

    public static Move Null => new(Coord.Null, Coord.Null);

    public Move(Coord from, Coord to) {
        From = from;
        To = to;
    }

    public string ToAlgabraicNotation() {
        return $"{From.ToAlgabraicNotation()}{To.ToAlgabraicNotation}";
    }

    public override string ToString() {
        return $"{{From:{From} To:{To}}}";
    }

    public override bool Equals(object obj) {
        return obj is Coord && Equals(obj);
    }

    public override int GetHashCode() {
        return From.GetHashCode() ^ To.GetHashCode();
    }

    public bool Equals(Move other) {
        return From == other.From && To == other.To;
    }

    public static bool operator ==(Move left, Move right) {
        return left.Equals(right);
    }

    public static bool operator !=(Move left, Move right) {
        return !(left == right);
    }
}
