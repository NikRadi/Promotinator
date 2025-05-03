namespace Promotinator.Engine;

public enum GameState {
    None,
    InProgress,
    WhiteWin,
    BlackWin,
    DrawByStalemate,
    DrawByDeadPosition,
    DrawByThreefoldRepitition,
    DrawByFiftyMoveRule,
}
