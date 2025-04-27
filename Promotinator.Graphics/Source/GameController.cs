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
    private Engine.Board _board;
    private BoardUI _boardUI;

    private IPlayer _whitePlayer;
    private IPlayer _blackPlayer;

    public GameController(float centerY) {
        _board = new Engine.Board("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

        int size = 600;
        Vector2 position = new(50, centerY - (size / 2));
        _boardUI = new(position, size);
        _boardUI.PlacePieces(_board);

        // TODO: HumanPlayer vs. HumanPlayer is buggy.
        // Both HumanPlayer instances subscribe to 'OnDragStarted' and 'OnDragEnded',
        // resulting in both of them trying to make a move each turn.
        _whitePlayer = new HumanPlayer(_boardUI, _board);
        _blackPlayer = new AIPlayer(_board);

        if (_board.Turn == Engine.Color.White) {
            _whitePlayer.StartMakingMove();
        }
        else {
            _blackPlayer.StartMakingMove();
        }

        _whitePlayer.OnMakeMove += HandleWhitePlayerMove;
        _blackPlayer.OnMakeMove += HandleBlackPlayerMove;

        UpdateBoardLockedColor();
        UpdateBoardOrientation();
    }

    public void Update() {
        _boardUI.Update();

//        if (_aiMoveTask.IsCompleted && !_isAIPaused) {
//            if (_aiMoveTask.IsFaulted) {
//                Console.WriteLine($"An error ocurred during AI move: {_aiMoveTask.Exception}");
//            }
//
//            bool canAIMoveWhite = _isWhitePlayerAI && _board.Turn == Engine.Color.White;
//            bool canAIMoveBlack = _isBlackPlayerAI && _board.Turn == Engine.Color.Black;
//            if (canAIMoveWhite || canAIMoveBlack) {
//                _aiMoveTask = TryAIMoveAsync();
//            }
//        }
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
