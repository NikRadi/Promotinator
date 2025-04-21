using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Promotinator.Graphics.UI;

public class SquareUI {
    public Vector2 Position {
        get { return _rectUI.Position; }
        set { _rectUI.Position = value; }
    }

    public Vector2 Size => _rectUI.Size;

    private readonly RectangleUI _rectUI;
    private bool _isLastMove;
    private bool _isLight;
    private bool _isPotentialMove;

    private static Color DarkColor { get; } = new(120, 80, 30);
    private static Color LightColor { get; } = new(230, 200, 85);
    private static Color LastMoveDarkColor { get; } = new(200, 200, 50);
    private static Color LastMoveLightColor { get; } = new(255, 255, 100);
    private static Color PotentialMoveDarkColor { get; } = new(30, 70, 120);
    private static Color PotentialColorLightColor { get; } = new(85, 190, 230);

    public SquareUI(Vector2 position, int size, bool isLightSquare) {
        _isLight = isLightSquare;
        _rectUI = new(position, new(size, size));
        UpdateColor();
    }

    public void Draw(SpriteBatch spriteBatch) {
        _rectUI.Draw(spriteBatch);
    }

    public void SetLastMoveHighlight(bool isLastMove) {
        _isLastMove = isLastMove;
        UpdateColor();
    }

    public void SetPotentialMoveHighlight(bool isPotentialMove) {
        _isPotentialMove = isPotentialMove;
        UpdateColor();
    }

    private void UpdateColor() {
        if (_isPotentialMove) {
            _rectUI.Color = _isLight ? PotentialColorLightColor : PotentialMoveDarkColor;
        }
        else if (_isLastMove) {
            _rectUI.Color = _isLight ? LastMoveLightColor : LastMoveDarkColor;
        }
        else {
            _rectUI.Color = _isLight ? LightColor : DarkColor;
        }
    }
}
