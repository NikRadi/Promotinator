using System;
using System.Collections.Generic;
using Promotinator.Graphics.UI;
using Promotinator.Graphics.Util;

namespace Promotinator.Graphics.Player;

public class HumanPlayer : IPlayer {
    public event EventHandler<Engine.Move> OnMakeMove;

    private bool _canMakeMove;
    private Engine.Board _board;
    private BoardUI _boardUI;
    private List<Engine.Move> _moves = [];
    private List<Coord> _highlightedSquares = [];

    public HumanPlayer(BoardUI boardUI, Engine.Board board) {
        _boardUI = boardUI;
        _boardUI.OnPieceDragStarted += OnDragStarted;
        _boardUI.OnPieceDragEnded += OnDragEnded;

        _board = board;
    }

    public void StartMakingMove() {
        _canMakeMove = true;
    }

    private void OnDragStarted(object sender, Coord coord) {
        _moves = Engine.MoveGenerator.GenerateMoves(_board);

        foreach (var move in _moves) {
            if (move.From.File == coord.File && move.From.Rank == coord.Rank) {
                Coord to = new(move.To.File, move.To.Rank);
                _boardUI.SetPotentialMoveHighlight(to, isHighlighted: true);
                _highlightedSquares.Add(to);
            }
        }
    }

    private void OnDragEnded(object sender, Move move) {
        foreach (var coord in _highlightedSquares) {
            _boardUI.SetPotentialMoveHighlight(coord, isHighlighted: false);
        }

        if (move == Move.Null) {
            return;
        }

        if (_canMakeMove) {
            foreach (var m in _moves) {
                bool isSameFrom = m.From.File == move.From.File && m.From.Rank == move.From.Rank;
                bool isSameTo = m.To.File == move.To.File && m.To.Rank == move.To.Rank;
                if (isSameFrom && isSameTo) {
                    OnMakeMove?.Invoke(this, m);
                    break;
                }
            }
        }

        _moves.Clear();
    }
}

