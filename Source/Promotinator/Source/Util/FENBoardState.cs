namespace Promotinator.Engine.Utils;

public struct FENBoardState {
    public Color Turn;
    public CastlingRights CastlingRights;
    public Piece?[] Pieces = new Piece?[8 * 8];
    public int? EnPassantSquare;

    public FENBoardState() { }
}