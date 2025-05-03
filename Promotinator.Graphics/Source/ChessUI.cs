using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Promotinator.Engine.Utils;
using Promotinator.Graphics.UI;

namespace Promotinator.Graphics;

public class ChessUI : Game {
    private const int ScreenHeight = 720;
    private const int ScreenWidth = 1280;

    private Color BackgroundColor { get; } = new(75, 75, 75);
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Tournament _tournament;

    public ChessUI() {
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics = new(this) {
            PreferredBackBufferHeight = ScreenHeight,
            PreferredBackBufferWidth = ScreenWidth,
        };
    }

    protected override void Initialize() {
        RectangleUI.Initialize(GraphicsDevice);

        float centerY = GraphicsDevice.Viewport.Height / 2;
        string[] fens = [
            FENUtil.StartPosition,
            FENUtil.StartPosition,
            FENUtil.StartPosition,
        ];

        _tournament = new(centerY, fens);

        base.Initialize();
    }

    protected override void Update(GameTime gameTime) {
        Input.Update();
        _tournament.Update();

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
