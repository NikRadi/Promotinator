using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Promotinator.UI.Players;

public struct MoveInfo {
    public Coord From;
    public Coord To;
}

public class GameController {
    private Engine.Board _board;
    private BoardUI _boardUI;
    private bool _isBlackPlayerAI;
    private bool _isWhitePlayerAI;
    private bool _isAIThinking;

    private List<Coord> _highlightedSquares = [];
    private Stack<MoveInfo> _history = new();

    public GameController(float centerY) {
        _board = new Engine.Board("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

        int size = 600;
        Vector2 position = new(50, centerY - (size / 2));
        _boardUI = new(this, position, size);
        _boardUI.PlacePieces(_board);

        _isBlackPlayerAI = false;
        _isWhitePlayerAI = true;

        UpdateBoardLockedColor();
        UpdateBoardOrientation();
    }

    public async Task Update() {
        _boardUI.Update();

        if (!_isAIThinking && ((_board.Turn == Engine.Color.White && _isWhitePlayerAI) ||_board.Turn == Engine.Color.Black && _isBlackPlayerAI)) {
            _isAIThinking = true;

            try {
                Task<Engine.Move> aiMoveTask = Engine.Search.FindBestMoveAsync(_board);
                Task minimumThinkTimeTask = Task.Delay(500);

                await Task.WhenAll(aiMoveTask, minimumThinkTimeTask);

                Engine.Move move = await aiMoveTask;
                MakeMove(move);
            } catch (Exception e) {
                Console.WriteLine($"An error ocurred during AI move: {e.Message}");
            }
            finally {
                _isAIThinking = false;
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch) {
        _boardUI.Draw(spriteBatch);
    }

    public void OnMouseDown(Coord coord) {
        List<Engine.Move> moves = Engine.MoveGenerator.GenerateMoves(_board);
        _highlightedSquares.Clear();

        foreach (Engine.Move move in moves) {
            if (move.From.File == coord.File && move.From.Rank == coord.Rank) {
                _highlightedSquares.Add(new(move.To.File, move.To.Rank));
            }
        }

        _boardUI.SetHighlightedSquares(_highlightedSquares, isHighlighted: true);
    }

    public void OnMouseRelease(Coord from, Coord to) {
        _boardUI.SetHighlightedSquares(_highlightedSquares, isHighlighted: false);
        _highlightedSquares.Clear();

        List<Engine.Move> moves = Engine.MoveGenerator.GenerateMoves(_board);
        int moveIndex = moves.FindIndex(m => m.From.File == from.File && m.From.Rank == from.Rank && m.To.File == to.File && m.To.Rank == to.Rank);

        if (moveIndex < 0) {
            return;
        }

        MakeMove(moves[moveIndex]);
    }

    private void MakeMove(Engine.Move move) {
        _board.MakeMove(move);
        _boardUI.PlacePieces(_board);

        if (_history.Count > 0) {
            MoveInfo lastMove = _history.Peek();
            _boardUI.SetMoveHighlight(lastMove.From, lastMove.To, isHighlighted: false);
        }

        Coord from = new(move.From.File, move.From.Rank);
        Coord to = new(move.To.File, move.To.Rank);
        _history.Push(new() { From = from, To = to });
        _boardUI.SetMoveHighlight(from, to, isHighlighted: true);

        UpdateBoardLockedColor();
        UpdateBoardOrientation();
    }

    private void UpdateBoardLockedColor() {
        _boardUI.AreWhitePiecesLocked = _isWhitePlayerAI || _board.Turn != Engine.Color.White;
        _boardUI.AreBlackPiecesLocked = _isBlackPlayerAI || _board.Turn != Engine.Color.Black;
    }

    private void UpdateBoardOrientation() {
        bool isWhiteBottom = (_isWhitePlayerAI && _isBlackPlayerAI) || (!_isWhitePlayerAI && _board.Turn == Engine.Color.White);
        _boardUI.SetPerspective(isWhiteBottom);
    }
}
