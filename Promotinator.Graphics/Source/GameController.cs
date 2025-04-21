using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Promotinator.Graphics.UI;
using Promotinator.Graphics.Util;

namespace Promotinator.Graphics.Players;

public struct MoveInfo {
    public Engine.Move Move;
    public Engine.BoardState State;
    public List<Engine.ScoredMove> Scores;
}

public class GameController {
    private Engine.Board _board;
    private BoardUI _boardUI;
    private bool _isBlackPlayerAI;
    private bool _isWhitePlayerAI;
    private bool _isAIPaused;
    private Task _aiMoveTask = Task.CompletedTask;

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

    public void Update() {
        HandleUserInput();
        _boardUI.Update();

        if (_aiMoveTask.IsCompleted && !_isAIPaused) {
            if (_aiMoveTask.IsFaulted) {
                Console.WriteLine($"An error ocurred during AI move: {_aiMoveTask.Exception}");
            }

            bool canAIMoveWhite = _isWhitePlayerAI && _board.Turn == Engine.Color.White;
            bool canAIMoveBlack = _isBlackPlayerAI && _board.Turn == Engine.Color.Black;
            if (canAIMoveWhite || canAIMoveBlack) {
                _aiMoveTask = TryAIMoveAsync();
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

        MakeMove(moves[moveIndex], []);
    }

    private void MakeMove(Engine.Move move, List<Engine.ScoredMove> scores) {
        _isAIPaused = false;

        Engine.BoardState state = _board.MakeMove(move);
        _boardUI.PlacePieces(_board);

        if (_history.Count > 0) {
            MoveInfo info = _history.Peek();
            Engine.Move m = info.Move;
            _boardUI.SetMoveHighlight(new(m.From.File, m.From.Rank), new(m.To.File, m.To.Rank), isHighlighted: false);
        }

        Coord from = new(move.From.File, move.From.Rank);
        Coord to = new(move.To.File, move.To.Rank);
        _history.Push(new() { Move = move, State = state, Scores = scores });
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

    private void HandleUserInput() {
        if (Input.IsLeftMouseButtonDown()) {
            _boardUI.OnMouseDown();
        }

        if (Input.IsLeftMouseButtonReleased()) {
            _boardUI.OnMouseReleased();
        }

        if (Input.IsKeyPressedOnce(Keys.Back) && _aiMoveTask.IsCompleted) {
            UndoLastMove();
        }

        if (Input.IsKeyPressedOnce(Keys.Space)) {
            _isAIPaused = false;
        }
    }

    private async Task TryAIMoveAsync() {
        Task<List<Engine.ScoredMove>> aiMoveTask = Engine.Search.FindBestMoveAsync(_board);
        Task minimumThinkTimeTask = Task.Delay(500);

        await Task.WhenAll(aiMoveTask, minimumThinkTimeTask);

        List<Engine.ScoredMove> moves = await aiMoveTask;
        MakeMove(moves[0].Move, moves);
    }

    private void UndoLastMove() {
        if (_history.Count == 0) {
            return;
        }

        _isAIPaused = true;

        MoveInfo info = _history.Pop();
        Engine.Move m = info.Move;
        _boardUI.SetMoveHighlight(new(m.From.File, m.From.Rank), new(m.To.File, m.To.Rank), isHighlighted: false);

        _board.UndoMove(info.Move, info.State);
        _boardUI.PlacePieces(_board);

        if (_history.Count > 0) {
            info = _history.Peek();
            m = info.Move;
            _boardUI.SetMoveHighlight(new(m.From.File, m.From.Rank), new(m.To.File, m.To.Rank), isHighlighted: true);
        }
    }
}
