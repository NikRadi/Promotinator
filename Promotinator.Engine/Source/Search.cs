namespace Promotinator.Engine;

public struct ScoredMove {
    public Move Move;
    public int Score;
}

public static class Search {
    public static async Task<List<ScoredMove>> FindBestMoveAsync(Board board) {
        return await Task.Run(() => FindBestMove(board));
    }

    public static List<ScoredMove> FindBestMove(Board board) {
        SearchDebug.ClearLog();

        List<ScoredMove> result = [];
        List<Move> moves = MoveGenerator.GenerateMoves(board);

        foreach (Move move in moves) {
            BoardState state = board.MakeMove(move);

            int score = Minimax(board, 3);
            result.Add(new() { Move = move, Score = score });

            SearchDebug.Log($"Move:{move} Score:{score}");

            board.UndoMove(move, state);
        }

        result.Sort((m1, m2) => m2.Score.CompareTo(m1.Score));
        SearchDebug.Log($"Best move: {result[0].Move}");
        return result;
    }

    private static int Minimax(Board board, int depth) {
        if (depth == 0) {
            return Eval.Score(board);
        }

        if (board.Turn == Color.White) {
            int score = int.MinValue;
            List<Move> moves = MoveGenerator.GenerateMoves(board);

            foreach (Move move in moves) {
                BoardState state = board.MakeMove(move);
                score = Math.Max(score, Minimax(board, depth - 1));
                board.UndoMove(move, state);
            }

            return score;
        }
        else {
            int score = int.MaxValue;
            List<Move> moves = MoveGenerator.GenerateMoves(board);

            foreach (Move move in moves) {
                BoardState state = board.MakeMove(move);
                score = Math.Min(score, Minimax(board, depth - 1));
                board.UndoMove(move, state);
            }

            return score;
        }
    }
}
