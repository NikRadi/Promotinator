using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Promotinator.UI;

public static class DebugInfo {
    public static int NumLegalMoves;
    public static SpriteFont Font;

    private static Vector2 Offset { get; } = new(800, 50);
    private static int _numLines;

    public static void LoadContent(ContentManager content) {
        Font = content.Load<SpriteFont>("Arial");
    }

    public static void Draw(SpriteBatch spriteBatch) {
        _numLines = 0;
        Write(spriteBatch, $"Legal Moves: {NumLegalMoves}");
    }

    private static void Write(SpriteBatch spriteBatch, string text) {
        Vector2 position = new Vector2(0, _numLines * 25) + Offset;
        spriteBatch.DrawString(Font, text, position, Color.Black);

        _numLines += 1;
    }
}
