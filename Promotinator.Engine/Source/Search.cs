using System.Diagnostics;

namespace Promotinator.Engine;

public struct ScoredMove {
    public Move Move;
    public int Score;
}

public static class Search {
    private struct Diagnostics {
        public int NumNodesVisited;
        public int NumNodesEvaluated;
        public bool IsCanceledDueToTime;
        public bool IsCanceledDueToDepthLimit;
    }

    private static bool _isDone;
    private static int _maxMilliseconds;
    private static Stopwatch _stopwatch = new();
    private static Diagnostics _diagnostics;

    public static async Task<Move> FindBestMoveAsync(Board board) {
        return await Task.Run(() => FindBestMove(board));
    }

    public static Move FindBestMove(Board board, int maxMilliseconds = 500) {
        SearchDebug.ClearLog();

        List<Move> moves = MoveGenerator.GenerateMoves(board);

        if (moves.Count == 1) {
            return moves[0];
        }

        Dictionary<Move, ScoredMove> cache = new();
        foreach (var move in moves) {
            cache[move] = new() { Move = move, Score = 0 };
        }

        // Iterative deepening
        int depth = 0;
        _isDone = false;
        _maxMilliseconds = maxMilliseconds;
        _stopwatch.Restart();

        if (moves.Count >= 0) {
            while (!_isDone) {
                SearchDebug.Log($"Iterative deepening {{depth:{depth + 1} turn:{board.Turn} time:{MillisecondsLeft()}ms}}");
                _diagnostics = new();

                foreach (Move move in moves) {
                    BoardState state = board.MakeMove(move);

                    int score = Minimax(board, depth);

                    board.UndoMove(move, state);

                    if (_isDone) {
                        break;
                    }

                    cache[move] = new() { Move = move, Score = score };

                    SearchDebug.Log($"({score}) {move}");
                }

                depth += 1;

                if (_diagnostics.IsCanceledDueToTime) {
                    SearchDebug.Log("Canceled due to time");
                }
                else if (_diagnostics.IsCanceledDueToDepthLimit) {
                    SearchDebug.Log("Canceled due to depth limit");
                }

                SearchDebug.Log($"Nodes visited: {_diagnostics.NumNodesVisited}");
                SearchDebug.Log($"Nodes evaluated: {_diagnostics.NumNodesEvaluated}");
                SearchDebug.Log("");
            }
        }

        Dictionary<int, List<Move>> scoreToMoves = new();
        bool isMaximizingPlayer = board.Turn == Color.White;
        int bestScore = isMaximizingPlayer ? int.MinValue : int.MaxValue;

        foreach (var item in cache) {
            if (scoreToMoves.TryGetValue(item.Value.Score, out List<Move> m)) {
                m.Add(item.Key);
            }
            else {
                scoreToMoves[item.Value.Score] = [item.Key];
            }

            if (isMaximizingPlayer) {
                if (item.Value.Score > bestScore) {
                    bestScore = item.Value.Score;
                }
            }
            else {
                if (item.Value.Score < bestScore) {
                    bestScore = item.Value.Score;
                }
            }
        }

        List<Move> bestMoves = scoreToMoves[bestScore];
        Move bestMove = bestMoves[new Random().Next(0, bestMoves.Count - 1)];
        SearchDebug.Log($"Best move: {bestMove}");

        return bestMove;
    }

    private static int Minimax(Board board, int depth) {
        if (MillisecondsLeft() <= 0) {
            _isDone = true;
            _stopwatch.Stop();
            _diagnostics.IsCanceledDueToTime = true;
            return 0;
        }

        _diagnostics.NumNodesVisited += 1;

        if (depth == 0) {
            _diagnostics.NumNodesEvaluated += 1;
            _diagnostics.IsCanceledDueToDepthLimit = true;
            return Eval.Score(board);
        }

        List<Move> moves = MoveGenerator.GenerateMoves(board);
        Move bestMove = moves[0];
        int bestScore;

        if (board.Turn == Color.White) {
            bestScore = int.MinValue;

            foreach (Move move in moves) {
                BoardState state = board.MakeMove(move);
                int newScore = Minimax(board, depth - 1);
                board.UndoMove(move, state);

                bool isScoreMaximized = newScore > bestScore;

                if (isScoreMaximized) {
                    bestScore = newScore;
                    bestMove = move;
                }
            }
        }
        else {
            bestScore = int.MaxValue;

            foreach (Move move in moves) {
                BoardState state = board.MakeMove(move);
                int newScore = Minimax(board, depth - 1);
                board.UndoMove(move, state);

                bool isScoreMinimized = newScore < bestScore;

                if (isScoreMinimized) {
                    bestScore = newScore;
                    bestMove = move;
                }
            }
        }

        return bestScore;
    }

    private static long MillisecondsLeft() {
        return _maxMilliseconds - _stopwatch.ElapsedMilliseconds;
    }
}
