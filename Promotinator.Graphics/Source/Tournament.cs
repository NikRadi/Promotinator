using Microsoft.Xna.Framework.Graphics;

namespace Promotinator.Graphics;

public class Tournament {
    private string[] _fens;
    private int _currentFenIndex;
    private GameController _gameController;

    public Tournament(float centerY, string[] fens) {
        _fens = fens;
        _currentFenIndex = 0;
        _gameController = new(centerY);
    }

    public void Update() {
        _gameController.Update();

        if (_gameController.IsGameFinished()) {
            var fen = _fens[_currentFenIndex];
            _currentFenIndex += 1;

            _gameController.SetState(fen);
            _gameController.StartGame();
        }
    }

    public void Draw(SpriteBatch spriteBatch) {
        _gameController.Draw(spriteBatch);
    }
}
