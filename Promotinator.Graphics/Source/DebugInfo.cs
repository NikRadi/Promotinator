using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Promotinator.Graphics;

public static class DebugInfo {
    public static int NumLegalMoves;
    public static int NumWhiteWins;
    public static int NumBlackWins;
    public static int NumDraws;

    private static Vector2 Offset { get; } = new(800, 50);
    private static int _numLines;

    public static void Draw(SpriteBatch spriteBatch) {
        _numLines = 0;
        Write(spriteBatch, $"Legal Moves: {NumLegalMoves}");
        Write(spriteBatch, $"White Wins: {NumWhiteWins}");
        Write(spriteBatch, $"Black Wins: {NumBlackWins}");
        Write(spriteBatch, $"Draws: {NumDraws}");
    }

    private static void Write(SpriteBatch spriteBatch, string text) {
        Vector2 position = new Vector2(0, _numLines * 25) + Offset;
        TextRenderer.DrawText(spriteBatch, text, position);

        _numLines += 1;
    }
}
