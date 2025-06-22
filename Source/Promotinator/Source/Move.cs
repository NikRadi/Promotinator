namespace Promotinator.Engine;

public struct Move(int flags) : IEquatable<Move> {
    private readonly ushort _data = (ushort)(flags << FlagsOffset);

#pragma warning disable format
    private const int FlagsMask = 0x0f;
    private const int FlagsOffset = 12;

    public readonly int Flags => (_data >> FlagsOffset) & FlagsMask;

    public const int KingCastleFlag         = 0b0010;
    public const int QueenCastleFlag        = 0b0011;
    public const int EnPassantCaptureFlag   = 0b0101;
    public const int KnightPromotionFlag    = 0b1000;
    public const int BishopPromotionFlag    = 0b1001;
    public const int RookPromotionFlag      = 0b1010;
    public const int QueenPromotionFlag     = 0b1011;
    public const int PromotionFlag          = 0b1000;

    public readonly bool IsKingCastle       => Flags == KingCastleFlag;
    public readonly bool IsQueenCastle      => Flags == QueenCastleFlag;
    public readonly bool IsEnPassantCapture => Flags == EnPassantCaptureFlag;
    public readonly bool IsKnightPromotion  => Flags == KnightPromotionFlag;
    public readonly bool IsBishopPromotion  => Flags == BishopPromotionFlag;
    public readonly bool IsRookPromotion    => Flags == RookPromotionFlag;
    public readonly bool IsQueenPromotion   => Flags == QueenPromotionFlag;
    public readonly bool IsPromotion        => (Flags & QueenPromotionFlag) > 0;
#pragma warning restore format

    public Coord From;
    public Coord To;
    public Piece? CapturedPiece;

    public int FromIdx => Index(From.File, From.Rank);
    public int ToIdx => Index(To.File, To.Rank);

    public static int Index(int file, int rank) => rank * 8 + file;

    public override string ToString() {
        string str = $"{From.ToAlgabraicNotation()}{To.ToAlgabraicNotation()}";
        if (IsKnightPromotion) str += "n";
        else if (IsBishopPromotion) str += "b";
        else if (IsRookPromotion) str += "r";
        else if (IsQueenPromotion) str += "q";

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
            _data == other._data;
    }
}
