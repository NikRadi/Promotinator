namespace Promotinator.Engine;

public struct BoardState {
    public Coord? EnPassantSquare;
    public CastlingRights CastlingRights;
    public int FiftyMoveCounter;
}
