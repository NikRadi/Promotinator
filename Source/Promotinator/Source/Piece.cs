namespace Promotinator.Engine;

public struct Piece {
    public PieceType Type;
    public Color Color;

    public readonly bool Is(PieceType type) {
        return Type == type;
    }
}
