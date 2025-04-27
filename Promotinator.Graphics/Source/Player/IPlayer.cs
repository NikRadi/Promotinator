using System;

namespace Promotinator.Graphics.Player;

public interface IPlayer {
    event EventHandler<Engine.Move> OnMakeMove;

    void StartMakingMove();
}
