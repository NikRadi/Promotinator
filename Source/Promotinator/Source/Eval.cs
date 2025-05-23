namespace Promotinator.Engine;

public static class Eval {
    private static readonly int[] ValueOf = new int[(int) PieceType.Count];

    static Eval() {
        ValueOf[(int) PieceType.Queen] = 900;
        ValueOf[(int) PieceType.Rook] = 500;
        ValueOf[(int) PieceType.Knight] = 300;
        ValueOf[(int) PieceType.Bishop] = 300;
        ValueOf[(int) PieceType.Pawn] = 100;
    }

    public static int Score(Board board) {
        int score = 0;

        for (int file = 0; file < 8; ++file) {
            for (int rank = 0; rank < 8; ++rank) {
                if (!board.Pieces[file, rank].HasValue) {
                    continue;
                }

                Piece piece = board.Pieces[file, rank].Value;
                int sign = piece.Color == Color.White ? 1 : -1;
                score += sign * ValueOf[(int) piece.Type];
            }
        }

        return score;
    }
}
