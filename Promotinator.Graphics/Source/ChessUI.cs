using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Promotinator.Graphics.Players;
using Promotinator.Graphics.UI;

namespace Promotinator.Graphics;

public class ChessUI : Game {
    private const int ScreenHeight = 720;
    private const int ScreenWidth = 1280;

    private Color BackgroundColor { get; } = new(75, 75, 75);
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private GameController _gameController;

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
        _gameController = new GameController(centerY);

        base.Initialize();
    }

    protected override void Update(GameTime gameTime) {
        Input.Update();
        _gameController.Update();

        if (Input.IsKeyDown(Keys.Escape)) {
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
        _gameController.Draw(_spriteBatch);
        DebugInfo.Draw(_spriteBatch);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
