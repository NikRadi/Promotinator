using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Promotinator.Graphics.UI;

namespace Promotinator.Graphics;

public class Tournament {
    private string[] _fens;
    private int _currentFenIndex;
    private GameController _gameController;

    public Tournament(float centerY, string[] fens) {
        _fens = fens;
        _currentFenIndex = 0;
        _gameController = new(centerY);

        _gameController.OnGameOver += HandleGameOver;
    }

    public async Task Update() {
        await _gameController.Update();

        bool hasMoreGamesLeft = _currentFenIndex < _fens.Length;

        if (!hasMoreGamesLeft) {
            Console.WriteLine("Tournament::Update() - tournament done");
            return;
        }

        if (!_gameController.IsStarted) {
            StartNextGame();
        }
    }

    public void Draw(SpriteBatch spriteBatch) {
        _gameController.Draw(spriteBatch);
    }

    private void StartNextGame() {
        var fen = _fens[_currentFenIndex];
        _currentFenIndex += 1;

        Console.WriteLine($"Tournament::StartNextGame() - starting game {_currentFenIndex} of {_fens.Length}");

        _gameController.SetBoardFEN(fen);
        _gameController.StartGame();
    }

    private void HandleGameOver(object sender, PlayerColor? winner) {
        Console.Write("Tournament::HandleGameOver() - ");
        if (winner.HasValue) {
            Console.WriteLine($"{winner.Value} wins");
        }
        else {
            Console.WriteLine("draw");
        }

        if (winner.HasValue) {
            if (winner.Value == PlayerColor.White) {
                DebugInfo.NumWhiteWins += 1;
            }
            else {
                DebugInfo.NumBlackWins += 1;
            }
        }
        else {
            DebugInfo.NumDraws += 1;
        }
    }
}
