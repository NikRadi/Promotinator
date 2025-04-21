using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Promotinator.Graphics.UI;

public enum PieceColor {
    White,
    Black,
}

public enum PieceType {
    King,
    Queen,
    Bishop,
    Knight,
    Rook,
    Pawn,
}

public class PieceUI {
    public PieceColor Color;
    public PieceType Type;
    public Vector2 Position;
    public int Size;

    private const int SpritesPerRow = 6;
    private const int SpritesPerColumn = 2;

    // Array of [PieceColor, PieceType].
    private static Rectangle[,] SourceRectangles { get; set; } = new Rectangle[SpritesPerColumn, SpritesPerRow];

    private static Texture2D SpriteSheet;

    public PieceUI(PieceColor color, PieceType type, Vector2 position, int size) {
        Color = color;
        Type = type;
        Position = position;
        Size = size;
    }

    public static void LoadContent(ContentManager content) {
        SpriteSheet = content.Load<Texture2D>("pieces");
        int width = SpriteSheet.Width / SpritesPerRow;
        int height = SpriteSheet.Height / SpritesPerColumn;

        SourceRectangles[(int) PieceColor.White, (int) PieceType.King]      = new(width * 0, 0, width, height);
        SourceRectangles[(int) PieceColor.White, (int) PieceType.Queen]     = new(width * 1, 0, width, height);
        SourceRectangles[(int) PieceColor.White, (int) PieceType.Bishop]    = new(width * 2, 0, width, height);
        SourceRectangles[(int) PieceColor.White, (int) PieceType.Knight]    = new(width * 3, 0, width, height);
        SourceRectangles[(int) PieceColor.White, (int) PieceType.Rook]      = new(width * 4, 0, width, height);
        SourceRectangles[(int) PieceColor.White, (int) PieceType.Pawn]      = new(width * 5, 0, width, height);

        SourceRectangles[(int) PieceColor.Black, (int) PieceType.King]      = new(width * 0, height, width, height);
        SourceRectangles[(int) PieceColor.Black, (int) PieceType.Queen]     = new(width * 1, height, width, height);
        SourceRectangles[(int) PieceColor.Black, (int) PieceType.Bishop]    = new(width * 2, height, width, height);
        SourceRectangles[(int) PieceColor.Black, (int) PieceType.Knight]    = new(width * 3, height, width, height);
        SourceRectangles[(int) PieceColor.Black, (int) PieceType.Rook]      = new(width * 4, height, width, height);
        SourceRectangles[(int) PieceColor.Black, (int) PieceType.Pawn]      = new(width * 5, height, width, height);
    }

    public void Draw(SpriteBatch spriteBatch) {
        Rectangle destination = new((int) Position.X, (int) Position.Y, Size, Size);
        Rectangle source = SourceRectangles[(int) Color, (int) Type];
        spriteBatch.Draw(SpriteSheet, destination, source, Microsoft.Xna.Framework.Color.White);
    }

    public override string ToString() {
        return $"{{Color:{Color} Type:{Type} Position:{Position}}}";
    }
}
