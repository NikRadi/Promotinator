using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Promotinator.Graphics.UI;

public class SquareUI {
    public bool IsHighlighted;
    public bool IsLightSquare;
    public bool WasLastMove;
    public string Text;

    public Vector2 Position {
        get { return _rectUI.Position; }
        set { _rectUI.Position = value; }
    }

    public Vector2 Size => _rectUI.Size;

    private readonly RectangleUI _rectUI;

    private static Color DarkSquareColor { get; } = new(120, 80, 30);
    private static Color LightSquareColor { get; } = new(230, 200, 85);
    private static Color HighlightedDarkSquareColor { get; } = new(30, 70, 120);
    private static Color HighlightedLightSquareColor { get; } = new(85, 190, 230);
    private static Color LastMoveDarkSquareColor { get; } = new(200, 200, 50);
    private static Color LastMoveLightSquareColor { get; } = new(255, 255, 100);

    public SquareUI(Vector2 position, int size, bool isLightSquare) {
        IsLightSquare = isLightSquare;
        _rectUI = new(position, new(size, size));
    }

    public void Draw(SpriteBatch spriteBatch) {
        if (IsHighlighted) {
            _rectUI.Color = IsLightSquare ? HighlightedLightSquareColor : HighlightedDarkSquareColor;
        }
        else if (WasLastMove) {
            _rectUI.Color = IsLightSquare ? LastMoveLightSquareColor : LastMoveDarkSquareColor;
        }
        else {
            _rectUI.Color = IsLightSquare ? LightSquareColor : DarkSquareColor;
        }

        _rectUI.Draw(spriteBatch);
    }
}
