using System.Collections.Generic;
using System.Threading.Tasks;

namespace Promotinator.Graphics.Player;

public class AIPlayer : IPlayer {
    private Engine.Board _board;

    public AIPlayer(Engine.Board board) {
        _board = board;
    }

    public Task<Engine.Move> StartMakingMove() {
        return Task.Run(GetMove);
    }

    private Engine.Move GetMove() {
        List<Engine.ScoredMove> moves = Engine.Search.FindBestMove(_board, 500);
        Engine.Move move = moves[0].Move;
        return move;
    }
}
