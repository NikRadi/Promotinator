using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
            "r1bqkbnr/pp3ppp/2n5/3pp3/3P1P2/2P5/PP4PP/RNBQKBNR w KQkq - 0 6",
            "r4rk1/1pq1n1pp/p1n2p2/1B1p4/Q2P2b1/2P2N2/PP1N2PP/2R1R1K1 w - - 0 16",
            "r1bq1rk1/ppp1p1bp/2n2pp1/3pPnB1/3P4/1BP2N1P/PP1N1PP1/R2QK2R w KQ - 0 11",
            "2k1r3/1pp4p/1p6/8/1PP1r1p1/P2Rb3/4KP1P/7R w - - 0 26",
            "rn2kb1r/ppp1pppp/8/3q4/6b1/5N2/PPPP1PPP/R1BQKB1R w KQkq - 2 6",
            "rnbqk1nr/1pp2ppp/1p1p4/4p3/4P3/2NP4/PPP2PPP/R2QKBNR w KQkq - 0 6",
            "rnbqk2r/ppp2ppp/3b1n2/3p4/3P4/2N2N2/PPP2PPP/R1BQKB1R w KQkq - 3 6",
            "r1bq1rk1/pp2bpp1/2n1pn1p/3p2B1/3P4/2NBPN2/PPQ2PPP/R3K2R w KQ - 0 11",
            "r1b1kb1r/ppp1p1pp/2nq1n2/3p2B1/3P4/8/PPP2PPP/RN1QKBNR w KQkq - 2 6",
            "r1bqkb1r/pp2pppp/2n2n2/3p4/3P4/2N2N2/PPP2PPP/R1BQKB1R w KQkq - 1 6",
            "r3kb1r/5ppp/pqn1p3/3p4/1p1PnBP1/1B3N1P/PPP1QP2/R3K2R w KQkq - 0 16",
            "rnbq1rk1/ppp1ppbp/3p1np1/8/3P1B2/2PBP3/PP3PPP/RN1QK1NR w KQ - 2 6",
            "rn2kb1r/pp2pppp/2pn4/3p1q2/3P4/NQ2PP2/PPP3PP/R1B1K1NR w KQkq - 1 11",
            "r1bqr1k1/ppp2ppp/3p1bn1/8/2BPP2P/2N2N2/PPP3P1/R2QK2R w KQ - 3 11",
            "r1bqkb1r/ppp2ppp/2n2n2/4p1B1/4p3/2NP1N2/PPP2PPP/R2QKB1R w KQkq - 0 6",
            "rnbq1rk1/ppp1bppp/3p1n2/3Pp3/4P3/2N2N2/PPP2PPP/R1BQKB1R w KQ - 5 6",
            "r1bq1rk1/pp3ppp/2np4/4p1b1/2B1P3/8/PPPQ1PPP/R1B1K2R w KQ - 0 11",
            "r1b2rk1/pp3p1p/3p2p1/4p1B1/2BnP2P/8/PP1K1PP1/3R3R w - - 2 16",
            "r1bqkb1r/pp3ppp/2n1pn2/2pp4/2PP4/2N1PN2/PP3PPP/R1BQKB1R w KQkq - 3 6",
            "r2qr1k1/ppb2p1p/2n2np1/2Ppp3/1P6/P1N1PP2/1BB2P1P/RQ3RK1 w - - 0 16",
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
        TextRenderer.LoadContent(Content);

        base.LoadContent();
    }

    protected override void Draw(GameTime gameTime) {
        GraphicsDevice.Clear(BackgroundColor);

        _spriteBatch.Begin();
        _tournament.Draw(_spriteBatch);

        TextRenderer.Draw(_spriteBatch);
        DebugInfo.Draw(_spriteBatch);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
