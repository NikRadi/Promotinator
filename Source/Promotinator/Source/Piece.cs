namespace Promotinator.Engine;

public struct Piece {
    public PieceType Type;
    public Color Color;

    public bool Is(PieceType type) {
        return Type == type;
    }
}
