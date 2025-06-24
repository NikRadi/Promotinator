namespace Promotinator.Engine;

[Flags]
public enum CastlingRights {
#pragma warning disable format
    None            = 0,
    WhiteKingside   = 1 << 0,
    WhiteQueenside  = 1 << 1,
    BlackKingside   = 1 << 2,
    BlackQueenside  = 1 << 3,
    WhiteBoth       = WhiteKingside | WhiteQueenside,
    BlackBoth       = BlackKingside | BlackQueenside,
    All             = WhiteBoth | BlackBoth
#pragma warning restore format
}
