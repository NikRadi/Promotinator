namespace Promotinator.Engine;

public struct Move(int flags) : IEquatable<Move> {
    private readonly ushort _data = (ushort)(flags << FlagsOffset);

#pragma warning disable format
    private const int FlagsMask = 0x0f;
    private const int FlagsOffset = 12;

    public readonly int Flags => (_data >> FlagsOffset) & FlagsMask;

    public const int EnPassantCaptureFlag = 0b0101;

    public readonly bool IsEnPassantCapture => (Flags & EnPassantCaptureFlag) == EnPassantCaptureFlag;
#pragma warning restore format

    public Coord From;
    public Coord To;
    public Piece? CapturedPiece;
    public bool IsKingsideCastling;
    public bool IsQueensideCastling;
    public PromotionType PromotionType;

    public int FromIdx => Index(From.File, From.Rank);
    public int ToIdx => Index(To.File, To.Rank);

    public static int Index(int file, int rank) => rank * 8 + file;

    public override string ToString() {
        string str = $"{From.ToAlgabraicNotation()}{To.ToAlgabraicNotation()}";
        if (PromotionType != PromotionType.None) {
            str += ToString(PromotionType);
        }

        return str;
    }

    public override int GetHashCode() {
        return From.GetHashCode() ^ To.GetHashCode();
    }

    public override bool Equals(object obj) {
        return obj is Move && Equals(obj);
    }

    public bool Equals(Move other) {
        return
            From == other.From &&
            To == other.To &&
            CapturedPiece.HasValue == other.CapturedPiece.HasValue &&
            IsEnPassantCapture == other.IsEnPassantCapture &&
            IsKingsideCastling == other.IsKingsideCastling &&
            IsQueensideCastling == other.IsQueensideCastling &&
            PromotionType == other.PromotionType;
    }

    private string ToString(PromotionType type) {
        switch (type) {
            case PromotionType.Queen: return "q";
            case PromotionType.Rook: return "r";
            case PromotionType.Knight: return "n";
            case PromotionType.Bishop: return "b";
            default: return string.Empty;
        }
    }
}
