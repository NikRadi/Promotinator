namespace Promotinator.Engine;

public struct Move {
    public Coord From;
    public Coord To;
    public Piece? CapturedPiece;
    public bool IsEnPassantCapture;
    public bool IsKingsideCastling;
    public bool IsQueensideCastling;
    public PromotionType PromotionType;

    public override string ToString() {
        string str = $"{From.ToAlgabraicNotation()}{To.ToAlgabraicNotation()}";
        if (PromotionType != PromotionType.None) {
            str += ToString(PromotionType);
        }

        return str;
    }

    private string ToString(PromotionType type) {
        switch (type) {
            case PromotionType.Queen: return "q";
            case PromotionType.Rook : return "r";
            case PromotionType.Knight: return "n";
            case PromotionType.Bishop: return "b";
            default: return null;
        }
    }
}
