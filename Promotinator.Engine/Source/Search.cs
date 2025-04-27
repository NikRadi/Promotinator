using System.Diagnostics;

namespace Promotinator.Engine;

public struct ScoredMove {
    public Move Move;
    public int Score;
}

public static class Search {
    private static bool _isDone;
    private static int _maxMilliseconds;
    private static Stopwatch _stopwatch = new();

    public static async Task<List<ScoredMove>> FindBestMoveAsync(Board board) {
        return await Task.Run(() => FindBestMove(board));
    }

    public static List<ScoredMove> FindBestMove(Board board, int maxMilliseconds = 500) {
        SearchDebug.ClearLog();

        List<ScoredMove> result = [];
        List<Move> moves = MoveGenerator.GenerateMoves(board);

        if (moves.Count == 1) {
            result.Add(new() { Move = moves[0]} );
            return result;
        }

        Dictionary<Move, ScoredMove> cache = new();
        foreach (var move in moves) {
            cache[move] = new() { Move = move, Score = 0 };
        }

        // Iterative deepening
        int depth = 1;
        _isDone = false;
        _maxMilliseconds = maxMilliseconds;
        _stopwatch.Restart();

        while (!_isDone) {
            foreach (Move move in moves) {
                BoardState state = board.MakeMove(move);

                int score = Minimax(board, depth);
                cache[move] = new() { Move = move, Score = score };

                SearchDebug.Log($"Move:{move} Score:{score}");

                board.UndoMove(move, state);
            }

            depth += 1;
        }

        foreach (var move in cache.Values) {
            result.Add(move);
        }

        result.Sort((m1, m2) => m2.Score.CompareTo(m1.Score));
        SearchDebug.Log($"Best move: {result[0].Move}");
        return result;
    }

    private static int Minimax(Board board, int depth) {
        if (_stopwatch.ElapsedMilliseconds >= _maxMilliseconds) {
            _isDone = true;
            return 0;
        }

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
