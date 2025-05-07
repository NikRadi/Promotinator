using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Promotinator.Graphics;

public static class TextRenderer {
    private static SpriteFont _font;
    private static List<Text> _texts = [];

    public static void LoadContent(ContentManager content) {
        _font = content.Load<SpriteFont>("Arial");
    }

    public static void Draw(SpriteBatch spriteBatch) {
        foreach (var text in _texts) {
            spriteBatch.DrawString(_font, text.Value, text.Position, Color.Black);
        }
    }

    public static void DrawText(SpriteBatch spriteBatch, string text, Vector2 position) {
        spriteBatch.DrawString(_font, text, position, Color.Black);
    }

    public static void Add(Text text) {
        _texts.Add(text);
    }

    public static float GetWidth(Text text) {
        return _font.MeasureString(text.Value).X;
    }

    public static float GetHeight(Text text) {
        return _font.MeasureString(text.Value).Y;
    }
}
