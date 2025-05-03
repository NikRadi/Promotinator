using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Promotinator.Graphics.Player;

public class AIPlayer : IPlayer {
    public event EventHandler<Engine.Move> OnMakeMove;

    private Engine.Board _board;

    public AIPlayer(Engine.Board board) {
        _board = board;
    }

    public void StartMakingMove() {
        new Task(() => {
            List<Engine.ScoredMove> moves = Engine.Search.FindBestMove(_board, 100);
            Engine.Move move = moves[0].Move;
            OnMakeMove?.Invoke(this, move);
        }).Start();
    }
}
