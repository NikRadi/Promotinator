namespace Promotinator.Engine;

public struct Move {
    public Coord From;
    public Coord To;
    public Piece? CapturedPiece;
    public bool IsEnPassantCapture;
    public bool IsKingsideCastling;
    public bool IsQueensideCastling;

    public override string ToString() {
        return $"{From.ToAlgabraicNotation()}{To.ToAlgabraicNotation()}";
    }
}
