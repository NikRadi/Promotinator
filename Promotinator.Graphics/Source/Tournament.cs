using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Promotinator.Graphics.UI;

namespace Promotinator.Graphics;

public class Tournament {
    private string[] _fens;
    private int _currentFenIndex;
    private bool _hasWrittenResultsToFile;
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
            if (!_hasWrittenResultsToFile) {
                Console.WriteLine("Tournament::Update() - tournament done, writing results to file");
                _hasWrittenResultsToFile = true;
                WriteResultsToFile();
            }

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
        if (Input.IsKeyPressedOnce(Keys.Space)) {
            Console.WriteLine($"Tournament::StartNextGame() - starting game {_currentFenIndex + 1} of {_fens.Length}");
            var fen = _fens[_currentFenIndex];
            _currentFenIndex += 1;

            _gameController.SetBoardFEN(fen);
            _gameController.StartGame();
        }
    }

    private void HandleGameOver(object sender, PlayerColor? winner) {
        Console.WriteLine($"Tournament::HandleGameOver() - winner is {winner}");

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

    private void WriteResultsToFile() {
        // TODO
    }
}
