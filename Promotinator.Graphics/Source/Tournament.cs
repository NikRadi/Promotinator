using System;
using Microsoft.Xna.Framework.Graphics;

namespace Promotinator.Graphics;

public class Tournament {
    private string[] _fens;
    private int _currentFenIndex;
    private bool _isStarted;
    private GameController _gameController;

    public Tournament(float centerY, string[] fens) {
        _fens = fens;
        _currentFenIndex = 0;
        _gameController = new(centerY);
    }

    public void Update() {
        if (!_isStarted) {
            StartNextRound();
            _gameController.Update();
            _isStarted = true;
            return;
        }

        _gameController.Update();

        var state = _gameController.State;
        var isWhiteWin = state == Engine.GameState.WhiteWin;
        var isBlackWin = state == Engine.GameState.BlackWin;
        var isDraw =
            state == Engine.GameState.DrawByDeadPosition ||
            state == Engine.GameState.DrawByFiftyMoveRule ||
            state == Engine.GameState.DrawByStalemate ||
            state == Engine.GameState.DrawByThreefoldRepitition;

        var isGameFinished = isWhiteWin || isBlackWin || isDraw;

        if (isGameFinished) {
            _isStarted = true;

            if (isWhiteWin) {
                DebugInfo.NumWhiteWins += 1;
            }
            else if (isBlackWin) {
                DebugInfo.NumBlackWins += 1;
            }
            else if (isDraw) {
                DebugInfo.NumDraws += 1;
            }

            StartNextRound();
        }
    }

    public void Draw(SpriteBatch spriteBatch) {
        _gameController.Draw(spriteBatch);
    }

    private void StartNextRound() {
        Console.WriteLine("Tournament: starting next round");

        var fen = _fens[_currentFenIndex];
        _currentFenIndex += 1;

        _gameController.SetState(fen);
        _gameController.StartGame();
    }
}
