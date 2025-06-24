namespace Promotinator.Engine;

public struct Move(int from, int to, int flags) : IEquatable<Move> {
    // https://www.chessprogramming.org/Encoding_Moves
    // An ushort is 16 bits. We encode a move using bit fields:
    // ---------------------------------------------
    // Bits                     Description
    // ---------------------------------------------
    // 0000 0000 0011 1111      To square   (6 bits)
    // 0000 1111 1100 0000      From square (6 bits)
    // 1111 0000 0000 0000      Flags       (4 bits)
    // ---------------------------------------------
    private readonly ushort _data = (ushort)((flags << FlagsOffset) | (from << FromSquareOffset) | (to << ToSquareOffset));

#pragma warning disable format
    private const int SquareMask            = 0b1111_11;
    private const int FlagsMask             = 0b1111;
    private const int PromotionFlagsMask    = 0b0011;

    private const int ToSquareOffset        =  0;
    private const int FromSquareOffset      =  6;
    private const int FlagsOffset           = 12;

    public readonly int ToSquare            => (_data >> ToSquareOffset)    & SquareMask;
    public readonly int FromSquare          => (_data >> FromSquareOffset)  & SquareMask;
    public readonly int Flags               => (_data >> FlagsOffset)       & FlagsMask;

    // 0011 0000 0000 0000
    public readonly int PromotionFlags      => (_data >> FlagsOffset) & PromotionFlagsMask;

    public const int QuietMoveFlag          = 0b0000;
    public const int DoublePawnPushFlag     = 0b0001;
    public const int KingCastleFlag         = 0b0010;
    public const int QueenCastleFlag        = 0b0011;
    public const int CaptureFlag            = 0b0100;
    public const int EnPassantCaptureFlag   = 0b0101;
    public const int PromotionFlag          = 0b1000;
    public const int KnightPromotionFlag    = 0b00;
    public const int BishopPromotionFlag    = 0b01;
    public const int RookPromotionFlag      = 0b10;
    public const int QueenPromotionFlag     = 0b11;

    public readonly bool IsQuietMove        => Flags == QuietMoveFlag;
    public readonly bool IsDoublePawnPush   => Flags == DoublePawnPushFlag;
    public readonly bool IsKingCastle       => Flags == KingCastleFlag;
    public readonly bool IsQueenCastle      => Flags == QueenCastleFlag;
    public readonly bool IsCapture          => (Flags & CaptureFlag) != 0;
    public readonly bool IsEnPassantCapture => Flags == EnPassantCaptureFlag;
    public readonly bool IsPromotion        => (Flags & PromotionFlag) != 0;

    // Remember to check `IsPromotion` before using these.
    public readonly bool IsKnightPromotion  => PromotionFlags == KnightPromotionFlag;
    public readonly bool IsBishopPromotion  => PromotionFlags == BishopPromotionFlag;
    public readonly bool IsRookPromotion    => PromotionFlags == RookPromotionFlag;
    public readonly bool IsQueenPromotion   => PromotionFlags == QueenPromotionFlag;
#pragma warning restore format

    public Coord From;
    public Coord To;
    public Piece? CapturedPiece;

    public int FromIdx => Index(From.File, From.Rank);
    public int ToIdx => Index(To.File, To.Rank);

    public static int Index(int file, int rank) => rank * 8 + file;

    public override string ToString() {
        string str = $"{From.ToAlgabraicNotation()}{To.ToAlgabraicNotation()}";
        if (IsPromotion) {
            if (IsKnightPromotion) str += "n";
            else if (IsBishopPromotion) str += "b";
            else if (IsRookPromotion) str += "r";
            else if (IsQueenPromotion) str += "q";
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
            _data == other._data;
    }
}
