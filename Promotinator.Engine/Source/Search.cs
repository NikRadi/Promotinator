namespace Promotinator.Engine;

public static class Search {
    public static async Task<Move> FindBestMoveAsync(Board board) {
        return await Task.Run(() => FindBestMove(board));
    }

    public static Move FindBestMove(Board board) {
        List<Move> moves = MoveGenerator.GenerateMoves(board);
        return moves[new Random().Next(0, moves.Count)];
    }
}
