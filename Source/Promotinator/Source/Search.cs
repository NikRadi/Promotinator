using System.Diagnostics;

namespace Promotinator.Engine;

public struct ScoredMove {
    public Move Move;
    public int Score;
    public string Debug;
}

public static class Search {
    private struct Diagnostics {
        public int NumNodesVisited;
        public int NumNodesEvaluated;
    }

    private static bool _isSearchCancelled;
    private static int _maxMilliseconds;
    private static Stopwatch _stopwatch = new();
    private static Diagnostics _diagnostics;

    public static async Task<Move> FindBestMoveAsync(Board board) {
        return await Task.Run(() => FindBestMove(board));
    }

    public static Move FindBestMove(Board board, int maxMilliseconds = 200) {
        SearchDebug.Log("## Finding best move ##");
        List<Move> moves = MoveGenerator.GenerateMoves(board);

        if (moves.Count == 1) {
            SearchDebug.Log($"Returning only move: {moves[0]}");
            return moves[0];
        }

        // Iterative deepening
        int maxDepth = 128;
        int depth = 0;
        _isSearchCancelled = false;
        _maxMilliseconds = maxMilliseconds;

        bool isMaximizingPlayer = board.Turn == Color.White;
        int bestScore = isMaximizingPlayer ? int.MinValue : int.MaxValue;
        int cancelledMoveIndex = -1;
        ScoredMove result = new() { Score = bestScore, Debug = "", Move = moves[0] };
        _stopwatch.Restart();

        if (moves.Count >= 0) {
            while (!_isSearchCancelled && depth < maxDepth) {
                ScoredMove bestResult = new() { Score = bestScore, Debug = "None" };
                SearchDebug.Log($"Iterative deepening {{depth:{depth + 1} turn:{board.Turn} time:{MillisecondsLeft()}ms}}");
                _diagnostics = new();

                for (int i = 0; i < moves.Count; ++i) {
                    Move move = moves[i];
                    BoardState state = board.MakeMove(move);
                    var scoredMove = Minimax(board, depth);
                    board.UndoMove(move, state);

                    if (_isSearchCancelled) {
                        cancelledMoveIndex = i;
                        break;
                    }

                    if (isMaximizingPlayer) {
                        if (scoredMove.Score >= bestResult.Score) {
                            bestResult = scoredMove;
                            bestResult.Move = move;
                            bestResult.Debug = $"(Score:{scoredMove.Score}) {move}{scoredMove.Debug}";
                        }
                    }
                    else {
                        if (scoredMove.Score <= bestResult.Score) {
                            bestResult = scoredMove;
                            bestResult.Move = move;
                            bestResult.Debug = $"(Score:{scoredMove.Score}) {move}{scoredMove.Debug}";
                        }
                    }
                }

                depth += 1;

                if (!_isSearchCancelled) {
                    result = bestResult;
                }
                else {
                    SearchDebug.Log($"Search cancelled (move {cancelledMoveIndex}/{moves.Count})");
                }

                SearchDebug.Log(result.Debug);
                SearchDebug.Log($"Time elapsed: {_stopwatch.ElapsedMilliseconds}ms");
                SearchDebug.Log($"Nodes visited: {_diagnostics.NumNodesVisited}");
                SearchDebug.Log($"Nodes evaluated: {_diagnostics.NumNodesEvaluated}");
                SearchDebug.Log("");
            }
        }

        SearchDebug.Log($"Best move: {result.Move}");
        SearchDebug.Log($"Score: {result.Score}");
        SearchDebug.Log("");
        SearchDebug.Log("");

        Debug.Assert(board.Pieces[result.Move.FromIdx].HasValue, $"Invalid search result: {result.Move}");
        Debug.Assert(board.Pieces[result.Move.FromIdx].Value.Color == board.Turn, $"Moving invalid piece");

        return result.Move;
    }

    private static ScoredMove Minimax(Board board, int depth) {
        int score = board.Turn == Color.White ? int.MinValue : int.MaxValue;
        var msLeft = MillisecondsLeft();
        ScoredMove result = new() { Score = score, Debug = "Error" };

        if (msLeft <= 0) {
            _isSearchCancelled = true;
            _stopwatch.Stop();
            result.Debug += " (time)";
            return result;
        }

        _diagnostics.NumNodesVisited += 1;

        if (depth == 0) {
            _diagnostics.NumNodesEvaluated += 1;
            result.Score = Eval.Score(board);
            result.Debug = $" (depth, {result.Score})";
            return result;
        }

        List<Move> moves = MoveGenerator.GenerateMoves(board);

        if (moves.Count == 0) {
            _diagnostics.NumNodesEvaluated += 1;

            if (board.IsKingInCheck()) {
                result.Debug = " (check mate)";
                return result;
            }

            result.Score = 0;
            result.Debug = " (no moves)";
            return result;
        }

        if (board.Turn == Color.White) {
            foreach (Move move in moves) {
                BoardState state = board.MakeMove(move);
                var scoredMove = Minimax(board, depth - 1);
                board.UndoMove(move, state);

                if (scoredMove.Score >= result.Score) {
                    result = scoredMove;
                    result.Move = move;
                    result.Debug = $" {move}{result.Debug}";
                }
            }
        }
        else {
            foreach (Move move in moves) {
                BoardState state = board.MakeMove(move);
                var scoredMove = Minimax(board, depth - 1);
                board.UndoMove(move, state);

                if (scoredMove.Score <= result.Score) {
                    result = scoredMove;
                    result.Move = move;
                    result.Debug = $" {move}{result.Debug}";
                }
            }
        }

        return result;
    }

    private static long MillisecondsLeft() {
        return _maxMilliseconds - _stopwatch.ElapsedMilliseconds;
    }
}
