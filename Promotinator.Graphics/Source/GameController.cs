using System;
using System.Collections.Generic;
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
    public Engine.GameState State => _board.GetState();

    private Engine.Board _board;
    private BoardUI _boardUI;

    private IPlayer _whitePlayer;
    private IPlayer _blackPlayer;

    public GameController(float centerY = 0) {
        _board = new();

        int size = 600;
        Vector2 position = new(50, centerY - (size / 2));
        _boardUI = new(position, size);

        _whitePlayer = new AIPlayer(_board);
        _whitePlayer.OnMakeMove += HandleWhitePlayerMove;

        _blackPlayer = new AIPlayer(_board);
        _blackPlayer.OnMakeMove += HandleBlackPlayerMove;
    }

    public void SetState(string fen) {
        _board.SetState(fen);
        _boardUI.PlacePieces(_board);
    }

    public void StartGame() {
        if (_board.Turn == Engine.Color.White) {
            Console.WriteLine("GameController: white starting game");
            _whitePlayer.StartMakingMove();
        }
        else if (_board.Turn == Engine.Color.Black) {
            Console.WriteLine("GameController: black starting game");
            _blackPlayer.StartMakingMove();
        }
    }

    public void Update() {
        _boardUI.Update();
    }

    public void Draw(SpriteBatch spriteBatch) {
        _boardUI.Draw(spriteBatch);
    }

    private void MakeMove(Engine.Move move) {
        _board.MakeMove(move);
        _boardUI.PlacePieces(_board);

        Coord from = new(move.From.File, move.From.Rank);
        Coord to = new(move.To.File, move.To.Rank);
        _boardUI.SetLastMove(from, to);

        UpdateBoardLockedColor();
        UpdateBoardOrientation();
    }

    private void UpdateBoardLockedColor() {
        _boardUI.AreWhitePiecesLocked = _whitePlayer is not HumanPlayer || _board.Turn != Engine.Color.White;
        _boardUI.AreBlackPiecesLocked = _blackPlayer is not HumanPlayer || _board.Turn != Engine.Color.Black;
    }

    private void UpdateBoardOrientation() {
        bool isWhiteAI = _whitePlayer is not HumanPlayer;
        bool isBlackAI = _blackPlayer is not HumanPlayer;
//        bool isAIOnly = isWhiteAI && isBlackAI;
        _boardUI.SetPerspective(isWhiteBottom: !isWhiteAI || isBlackAI);
    }

    public void HandleWhitePlayerMove(object sender, Engine.Move move) {
        MakeMove(move);
        _blackPlayer.StartMakingMove();
    }

    public void HandleBlackPlayerMove(object sender, Engine.Move move) {
        MakeMove(move);
        _whitePlayer.StartMakingMove();
    }
}
