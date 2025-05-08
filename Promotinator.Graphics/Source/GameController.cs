using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Promotinator.Graphics.Player;
using Promotinator.Graphics.UI;
using Promotinator.Graphics.Util;

namespace Promotinator.Graphics;

public class GameController {
    public event EventHandler<PlayerColor?> OnGameOver;

    public bool IsStarted => _isStarted;

    private Engine.Board _board;
    private BoardUI _boardUI;
    private IPlayer _whitePlayer;
    private IPlayer _blackPlayer;
    private Task<Engine.Move> _moveTask;

    private List<(Engine.Move Move, Engine.BoardState State)> _history = [];
    private int _historyIndex = -1;

    private bool _isPaused = true;
    private bool _isStarted;

    public GameController(float centerY) {
        _board = new();

        int size = 600;
        Vector2 position = new(50, centerY - (size / 2));
        _boardUI = new(position, size);

        _whitePlayer = new AIPlayer(_board);
        _blackPlayer = new AIPlayer(_board);

        Engine.SearchDebug.ClearLog();
    }

    public async Task Update() {
        _boardUI.Update();
        HandleInput();

        if (_isPaused || !IsStarted) {
            return;
        }

        if (_moveTask != null && !_moveTask.IsCompleted) {
            return;
        }

        _moveTask = _board.Turn == Engine.Color.White ? _whitePlayer.StartMakingMove() : _blackPlayer.StartMakingMove();
        Engine.Move move = await _moveTask;
        MakeMove(move);

        var state = _board.GetState();

        var isWhiteWin = state == Engine.GameState.WhiteWin;
        var isBlackWin = state == Engine.GameState.BlackWin;
        var isDraw =
            state == Engine.GameState.DrawByDeadPosition ||
            state == Engine.GameState.DrawByFiftyMoveRule ||
            state == Engine.GameState.DrawByStalemate ||
            state == Engine.GameState.DrawByThreefoldRepitition;

        bool isGameOver = isWhiteWin || isBlackWin || isDraw;

        if (isGameOver) {
            _isStarted = false;

            if (isWhiteWin) {
                OnGameOver?.Invoke(this, PlayerColor.White);
            }
            else if (isBlackWin) {
                OnGameOver?.Invoke(this, PlayerColor.Black);
            }
            else {
                OnGameOver?.Invoke(this, null);
            }
        }
    }

    public void StartGame() {
        _isStarted = true;
    }

    public void Draw(SpriteBatch spriteBatch) {
        _boardUI.Draw(spriteBatch);
    }

    public void SetBoardFEN(string fen) {
        _board.SetState(fen);
        _boardUI.PlacePieces(_board);

        _whitePlayer = new AIPlayer(_board);
        _blackPlayer = new AIPlayer(_board);

        _history.Clear();
        _historyIndex = -1;
    }

    private void MakeMove(Engine.Move move) {
        var state = _board.MakeMove(move);
        _boardUI.PlacePieces(_board);

        Coord from = new(move.From.File, move.From.Rank);
        Coord to = new(move.To.File, move.To.Rank);
        _boardUI.SetLastMove(from, to);

        _history.Add((move, state));
        _historyIndex += 1;
    }

    private void HandleInput() {
        if (Input.IsKeyPressedOnce(Keys.Space)) {
            _isPaused = !_isPaused;

            if (_historyIndex < _history.Count - 1) {
                for (int i = _historyIndex; i < _history.Count - 1; ++i) {
                    NextMove();
                }
            }
        }

        if (Input.IsKeyPressedOnce(Keys.Left)) {
            PreviousMove();
        }

        if (Input.IsKeyPressedOnce(Keys.Right)) {
            NextMove();
        }
    }

    private void PreviousMove() {
        if (_historyIndex < 0) {
            return;
        }

        (Engine.Move move, Engine.BoardState state) = _history[_historyIndex];
        _historyIndex -= 1;

        _board.UndoMove(move, state);
        _boardUI.PlacePieces(_board);

        if (_historyIndex >= 0) {
            var lastMove = _history[_historyIndex].Move;
            Coord from = new(lastMove.From.File, lastMove.From.Rank);
            Coord to = new(lastMove.To.File, lastMove.To.Rank);
            _boardUI.SetLastMove(from, to);
        }
        else {
            _boardUI.ResetLastMoveHighlight();
        }
    }

    private void NextMove() {
        if (_historyIndex >= _history.Count - 1) {
            return;
        }

        _historyIndex += 1;
        var move = _history[_historyIndex].Move;

        _board.MakeMove(move);
        _boardUI.PlacePieces(_board);

        Coord from = new(move.From.File, move.From.Rank);
        Coord to = new(move.To.File, move.To.Rank);
        _boardUI.SetLastMove(from, to);
    }
}
