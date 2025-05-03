using System.Threading.Tasks;

namespace Promotinator.Graphics.Player;

public interface IPlayer {
    Task<Engine.Move> StartMakingMove();
}
