using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Promotinator.Engine.Utils;
using Promotinator.Graphics.UI;

namespace Promotinator.Graphics;

public class ChessUI : Game {
    private const int WindowWidth = 1280;
    private const int WindowHeight = 720;

    private Color BackgroundColor { get; } = new(75, 75, 75);
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Tournament _tournament;

    public ChessUI() {
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics = new(this) {
            PreferredBackBufferWidth = WindowWidth,
            PreferredBackBufferHeight = WindowHeight,
        };
    }

    protected override void Initialize() {
        RectangleUI.Initialize(GraphicsDevice);

        float centerY = GraphicsDevice.Viewport.Height / 2;
        string[] fens = [
            FENUtil.StartPosition,
            "r1bqkbnr/pp3ppp/2n5/3pp3/3P1P2/2P5/PP4PP/RNBQKBNR w KQkq - 0 6",
            "r4rk1/1pq1n1pp/p1n2p2/1B1p4/Q2P2b1/2P2N2/PP1N2PP/2R1R1K1 w - - 0 16",
            "r1bq1rk1/ppp1p1bp/2n2pp1/3pPnB1/3P4/1BP2N1P/PP1N1PP1/R2QK2R w KQ - 0 11",
            "2k1r3/1pp4p/1p6/8/1PP1r1p1/P2Rb3/4KP1P/7R w - - 0 26"
        ];

        _tournament = new(centerY, fens);

        base.Initialize();
    }

    protected override async void Update(GameTime gameTime) {
        Input.Update();
        await _tournament.Update();

        if (Input.IsKeyPressedOnce(Keys.Escape)) {
            Exit();
        }

        base.Update(gameTime);
    }

    protected override void LoadContent() {
        _spriteBatch = new(GraphicsDevice);
        PieceUI.LoadContent(Content);
        DebugInfo.LoadContent(Content);

        base.LoadContent();
    }

    protected override void Draw(GameTime gameTime) {
        GraphicsDevice.Clear(BackgroundColor);

        _spriteBatch.Begin();
        _tournament.Draw(_spriteBatch);

        DebugInfo.Draw(_spriteBatch);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
