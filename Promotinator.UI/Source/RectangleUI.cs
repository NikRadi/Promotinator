using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Promotinator.UI;

public class RectangleUI {
    public Vector2 Position;
    public Vector2 Size;
    public Color Color;

    private static Texture2D Pixel;

    public RectangleUI(Vector2 position, Vector2 size) {
        Position = position;
        Size = size;
    }

    public RectangleUI(Vector2 position, Vector2 size, Color color) {
        Position = position;
        Size = size;
        Color = color;
    }

    public static void Initialize(GraphicsDevice graphicsDevice) {
        Pixel = new Texture2D(graphicsDevice, 1, 1);
        Pixel.SetData([Color.White]);
    }

    public void Draw(SpriteBatch spriteBatch) {
        Rectangle destination = new((int) Position.X, (int) Position.Y, (int) Size.X, (int) Size.Y);
        spriteBatch.Draw(Pixel, destination, Color);
    }
}
