using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Promotinator.Graphics.Player;
using Promotinator.Graphics.UI;
using Promotinator.Graphics.Util;

namespace Promotinator.Graphics;

public struct MoveInfo {
    public Engine.Move Move;
    public Engine.BoardState State;
    public List<Engine.ScoredMove> Scores;
}

public class GameController {
    public event EventHandler<PlayerColor?> OnGameOver;

    public bool IsStarted { get; private set; }

    private Engine.Board _board;
    private BoardUI _boardUI;
    private IPlayer _whitePlayer;
    private IPlayer _blackPlayer;
    private Task<Engine.Move> _moveTask;

    public GameController(float centerY) {
        _board = new();

        int size = 600;
        Vector2 position = new(50, centerY - (size / 2));
        _boardUI = new(position, size);

        _whitePlayer = new AIPlayer(_board);
        _blackPlayer = new AIPlayer(_board);
    }

    public async Task Update() {
        _boardUI.Update();

        if (!IsStarted) {
            return;
        }

        if (_moveTask != null && !_moveTask.IsCompleted) {
            return;
        }

        Engine.Move move;

        if (_board.Turn == Engine.Color.White) {
            _moveTask = _whitePlayer.StartMakingMove();
        }
        else {
            _moveTask = _blackPlayer.StartMakingMove();
        }

        move = await _moveTask;
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
            IsStarted = false;

            if (isWhiteWin) {
                OnGameOver?.Invoke(this, PlayerColor.White);
            }
            else if (isWhiteWin) {
                OnGameOver?.Invoke(this, PlayerColor.Black);
            }
            else {
                OnGameOver?.Invoke(this, null);
            }
        }
    }

    public void StartGame() {
        IsStarted = true;
    }

    public void Draw(SpriteBatch spriteBatch) {
        _boardUI.Draw(spriteBatch);
    }

    public void SetBoardFEN(string fen) {
        _board.SetState(fen);
        _boardUI.PlacePieces(_board);
    }

    private void MakeMove(Engine.Move move) {
        _board.MakeMove(move);
        _boardUI.PlacePieces(_board);

        Coord from = new(move.From.File, move.From.Rank);
        Coord to = new(move.To.File, move.To.Rank);
        _boardUI.SetLastMove(from, to);

//        UpdateBoardLockedColor();
//        UpdateBoardOrientation();
    }

//    private void UpdateBoardLockedColor() {
//        _boardUI.AreWhitePiecesLocked = _whitePlayer is not HumanPlayer || _board.Turn != Engine.Color.White;
//        _boardUI.AreBlackPiecesLocked = _blackPlayer is not HumanPlayer || _board.Turn != Engine.Color.Black;
//    }
//
//    private void UpdateBoardOrientation() {
//        bool isWhiteAI = _whitePlayer is not HumanPlayer;
//        bool isBlackAI = _blackPlayer is not HumanPlayer;
//        bool isAIOnly = isWhiteAI && isBlackAI;
//        _boardUI.SetPerspective(isWhiteBottom: !isWhiteAI || isBlackAI);
//    }
}
