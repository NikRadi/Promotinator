using System.Threading.Tasks;

namespace Promotinator.Graphics.Player;

public class AIPlayer : IPlayer {
    private Engine.Board _board;

    public AIPlayer(Engine.Board board) {
        _board = board;
    }

    public Task<Engine.Move> StartMakingMove() {
        return Engine.Search.FindBestMoveAsync(_board);
    }
}
