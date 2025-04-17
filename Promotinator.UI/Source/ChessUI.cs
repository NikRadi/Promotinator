using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Promotinator.UI;

public class ChessUI : Game {
    private const int SCREEN_HEIGHT = 720;
    private const int SCREEN_WIDTH = 1280;

    private Color BackgroundColor { get; } = new(75, 75, 75);
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private BoardUI _boardUI;

    public ChessUI() {
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics = new(this) {
            PreferredBackBufferHeight = SCREEN_HEIGHT,
            PreferredBackBufferWidth = SCREEN_WIDTH,
        };
    }

    protected override void Initialize() {
        RectangleUI.Initialize(GraphicsDevice);

        int size = 600;
        float centerY = (GraphicsDevice.Viewport.Height - size) / 2;
        Vector2 position = new(50, centerY);
        _boardUI = new(position, size);

        base.Initialize();
    }

    protected override void Update(GameTime gameTime) {
        Input.Update();
        _boardUI.Update();

        if (Input.IsKeyDown(Keys.Escape)) {
            Exit();
        }

        base.Update(gameTime);
    }

    protected override void LoadContent() {
        _spriteBatch = new(GraphicsDevice);
        Piece.LoadContent(Content);
        DebugInfo.LoadContent(Content);

        base.LoadContent();
    }

    protected override void Draw(GameTime gameTime) {
        GraphicsDevice.Clear(BackgroundColor);

        _spriteBatch.Begin();
        _boardUI.Draw(_spriteBatch);
        DebugInfo.Draw(_spriteBatch);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
