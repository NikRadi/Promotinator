namespace Promotinator.Engine.Utils;

public static class FENUtil {
    public const string StartPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    public static FENBoardState ParseFEN(string fen) {
        FENBoardState state = new();
        string[] parts = fen.Split(' ');
        string pieces = parts[0];
        string turn = parts[1];
        string castling = parts[2];
        string enPassant = parts[3];

        state.Pieces = ParsePieces(pieces);
        state.Turn = ParseTurn(turn);
        state.CastlingRights = ParseCastlingRights(castling);
        state.EnPassantSquare = ParseEnPassant(enPassant);

        return state;
    }

    private static Piece?[,] ParsePieces(string fenPieces) {
        Piece?[,] pieces = new Piece?[8, 8];
        string[] ranks = fenPieces.Split('/');

        for (int rank = 0; rank < 8; ++rank) {
            int file = 0;
            foreach (char c in ranks[rank]) {
                if (char.IsDigit(c)) {
                    file += int.Parse(c.ToString());
                }
                else {
                    pieces[file, 7 - rank] = ParsePiece(c);
                    file += 1;
                }
            }
        }

        return pieces;
    }

    private static Color ParseTurn(string fenTurn) {
        switch (fenTurn[0]) {
            case 'b': return Color.Black;
            case 'w': return Color.White;
            default: throw new ArgumentException($"Failed parsing FEN turn: {fenTurn}");
        }
    }

    private static CastlingRights ParseCastlingRights(string fenCastling) {
        CastlingRights castling = CastlingRights.None;

        if (fenCastling.Contains('K')) castling |= CastlingRights.WhiteKingside;
        if (fenCastling.Contains('Q')) castling |= CastlingRights.WhiteQueenside;
        if (fenCastling.Contains('k')) castling |= CastlingRights.BlackKingside;
        if (fenCastling.Contains('q')) castling |= CastlingRights.BlackQueenside;

        return castling;
    }

    private static Piece ParsePiece(char c) {
        switch (c) {
            case 'P': return new Piece() { Color = Color.White, Type = PieceType.Pawn };
            case 'B': return new Piece() { Color = Color.White, Type = PieceType.Bishop };
            case 'N': return new Piece() { Color = Color.White, Type = PieceType.Knight };
            case 'R': return new Piece() { Color = Color.White, Type = PieceType.Rook };
            case 'Q': return new Piece() { Color = Color.White, Type = PieceType.Queen };
            case 'K': return new Piece() { Color = Color.White, Type = PieceType.King };
            case 'p': return new Piece() { Color = Color.Black, Type = PieceType.Pawn };
            case 'b': return new Piece() { Color = Color.Black, Type = PieceType.Bishop };
            case 'n': return new Piece() { Color = Color.Black, Type = PieceType.Knight };
            case 'r': return new Piece() { Color = Color.Black, Type = PieceType.Rook };
            case 'q': return new Piece() { Color = Color.Black, Type = PieceType.Queen };
            case 'k': return new Piece() { Color = Color.Black, Type = PieceType.King };
            default: throw new ArgumentException($"Failed to parse FEN piece: {c}");
        }
    }

    private static Coord? ParseEnPassant(string fenEnPassant) {
        if (fenEnPassant[0] == '-') return null;
        return new Coord(fenEnPassant);
    }
}
