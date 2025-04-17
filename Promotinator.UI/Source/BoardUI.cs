using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Promotinator.Engine.Utils;

namespace Promotinator.UI;

// TODO: Need white and black POV. Somehow flip board for black.
public class BoardUI {
    // (file, rank), (0, 0) = a1, (7, 7) = h8.
    private Square[,] Squares { get; } = new Square[8, 8];
    private Piece[,] Pieces { get; } = new Piece[8, 8];

    private RectangleUI[] Borders { get; } = new RectangleUI[4];
    private Color BorderColor { get; } = new(100, 100, 100);

    private int SquareSize { get; }
    private int BorderSize { get; }
    private Rectangle Bounds { get; }
    private Piece _draggedPiece;
    private Coord _draggedFrom;
    private Engine.Board _board;
    private List<Engine.Move> _moves;
    private Stack<Engine.Move> _history;

    // Required to undo moves
    private Engine.Coord? _lastEnPassantSquare;
    private Engine.CastlingRights _lastCastlingRights;

    // Required to reset the last highlighted move.
    private Coord[] LastMoveCoords { get; } = new Coord[2];

    public BoardUI(Vector2 position, int size) {
        SquareSize = size / 8;
        BorderSize = size / 60;
        Bounds = new((int) position.X, (int) position.Y, size, size);

        _board = new Engine.Board("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8");
        _history = new();

        CreateBorders();
        CreateSquares();
        CreatePiecesFromBoard();

        UpdateMoves();
    }

    public void Update() {
        if (Input.IsLeftMouseButtonClick()) {
            HandleMouseClick();
        }

        if (Input.IsLeftMouseButtonReleased()) {
            HandleMouseRelease();
        }

        if (Input.IsKeyPressedOnce(Keys.Back)) {
            UndoLastMove();
        }

        if (_draggedPiece != null) {
            _draggedPiece.Position = Input.MousePosition - (new Vector2(SquareSize) / 2);
        }
    }

    public void Draw(SpriteBatch spriteBatch) {
        foreach (RectangleUI border in Borders) {
            border.Draw(spriteBatch);
        }

        foreach (Square square in Squares) {
            square.Draw(spriteBatch);
        }

        // The pieces must be drawn last to appear on top of everything else.
        foreach (Piece piece in Pieces) {
            piece?.Draw(spriteBatch);
        }
    }

    private void CreateBorders() {
        // Top
        Borders[0] = new(
            position: new(Bounds.Left - BorderSize, Bounds.Top - BorderSize),
            size: new(Bounds.Width + BorderSize * 2, BorderSize),
            color: BorderColor
        );

        // Bottom
        Borders[1] = new(
            position: new(Bounds.Left - BorderSize, Bounds.Bottom),
            size: new(Bounds.Width + BorderSize * 2, BorderSize),
            color: BorderColor
        );

        // Left
        Borders[2] = new(
            position: new(Bounds.Left - BorderSize, Bounds.Top),
            size: new(BorderSize, Bounds.Height),
            color: BorderColor
        );

        // Right
        Borders[3] = new(
            position: new(Bounds.Right, Bounds.Top),
            size: new(BorderSize, Bounds.Height),
            color: BorderColor
        );
    }

    private void CreateSquares() {
        for (int rank = 0; rank < 8; ++rank) {
            for (int file = 0; file < 8; ++file) {
                Squares[file, rank] = new(
                    position: ToPosition(file, rank),
                    size: SquareSize,
                    isLightSquare: (file + rank) % 2 == 1
                );
            }
        }
    }

    private void CreatePiecesFromBoard() {
        for (int file = 0; file < 8; ++file) {
            for (int rank = 0; rank < 8; ++rank) {
                Engine.Piece? piece = _board.Pieces[file, rank];
                if (piece.HasValue) {
                    PlacePiece(_board.Pieces[file, rank].Value, file, rank);
                }
            }
        }
    }

    private void PlacePiece(Engine.Piece piece, int file, int rank) {
        PieceColor color;
        if (piece.Color == Engine.Color.Black) color = PieceColor.Black;
        else if (piece.Color == Engine.Color.White) color = PieceColor.White;
        else throw new ArgumentException($"Invalid piece color: {piece.Color}");

        PieceType type;
        if (piece.Type == Engine.PieceType.King) type = PieceType.King;
        else if (piece.Type == Engine.PieceType.Queen) type = PieceType.Queen;
        else if (piece.Type == Engine.PieceType.Rook) type = PieceType.Rook;
        else if (piece.Type == Engine.PieceType.Bishop) type = PieceType.Bishop;
        else if (piece.Type == Engine.PieceType.Knight) type = PieceType.Knight;
        else if (piece.Type == Engine.PieceType.Pawn) type = PieceType.Pawn;
        else throw new ArgumentException($"Invalid piece type: {piece.Type}");

        PlacePiece(color, type, file, rank);
    }

    private void PlacePiece(PieceColor color, PieceType type, int file, int rank) {
        Pieces[file, rank] = new(
            color: color,
            type: type,
            position: ToPosition(file, rank),
            size: SquareSize
        );
    }

    private void UpdateMoves() {
        _moves = Engine.MoveGenerator.GenerateMoves(_board);
        DebugInfo.NumLegalMoves = _moves.Count;
    }

    private void HandleMouseClick() {
        if (!IsInBoard(Input.MousePosition)) {
            return;
        }

        if (_draggedPiece == null) {
            Coord coord = ToCoord(Input.MousePosition);
            _draggedPiece = Pieces[coord.File, coord.Rank];
            if (_draggedPiece != null) {
                _draggedFrom = coord;
                SetHighlightedMoves(isHighlighted: true);
            }
        }
    }

    private void HandleMouseRelease() {
        if (_draggedPiece == null) {
            return;
        }

        Coord coord = ToCoord(Input.MousePosition);
        _draggedPiece.Position = ToPosition(coord.File, coord.Rank);

        if (IsInBoard(Input.MousePosition) && IsLegalMove(_draggedFrom, coord)) {
            StopLastMoveHighlight();

            // Highlighting move
            Squares[_draggedFrom.File, _draggedFrom.Rank].WasLastMove = true;
            Squares[coord.File, coord.Rank].WasLastMove = true;
            LastMoveCoords[0] = coord;
            LastMoveCoords[1] = _draggedFrom;

            // Update pieces
            Pieces[_draggedFrom.File, _draggedFrom.Rank] = null;
            Pieces[coord.File, coord.Rank] = _draggedPiece;

            // Update board state
            Engine.Move move = _moves.Find(m => m.From.File == _draggedFrom.File && m.From.Rank == _draggedFrom.Rank && m.To.File == coord.File && m.To.Rank == coord.Rank);
            _lastEnPassantSquare = _board.EnPassantSquare;
            _lastCastlingRights = _board.CastlingRights;

            _board.MakeMove(move);
            _history.Push(move);

            ResetPiecesFromBoard();
        }
        else {
            _draggedPiece.Position = ToPosition(_draggedFrom.File, _draggedFrom.Rank);
        }

        _draggedPiece = null;
        SetHighlightedMoves(isHighlighted: false);
        UpdateMoves();
    }

    private void UndoLastMove() {
        if (_history.Count == 0) {
            return;
        }

        // Undo in board state.
        Engine.Move move = _history.Pop();
        _board.UndoMove(move, _lastEnPassantSquare, _lastCastlingRights);

        // Undo in UI.
        UpdateMoves();
        ResetPiecesFromBoard();
        StopLastMoveHighlight();
    }

    private void ResetPiecesFromBoard() {
        Array.Clear(Pieces);
        CreatePiecesFromBoard();
    }

    private void SetHighlightedMoves(bool isHighlighted) {
        foreach (Engine.Move move in _moves.FindAll(m => m.From.File == _draggedFrom.File && m.From.Rank == _draggedFrom.Rank)) {
            Squares[move.To.File, move.To.Rank].IsHighlighted = isHighlighted;
        }
    }

    private void StopLastMoveHighlight() {
        Squares[LastMoveCoords[0].File, LastMoveCoords[0].Rank].WasLastMove = false;
        Squares[LastMoveCoords[1].File, LastMoveCoords[1].Rank].WasLastMove = false;
    }

    private bool IsLegalMove(Coord from, Coord to) {
        return _moves.Exists(m => m.From.File == from.File && m.From.Rank == from.Rank && m.To.File == to.File && m.To.Rank == to.Rank);
    }

    private Coord ToCoord(Vector2 position) {
        return new(ToRank(position), ToFile(position));
    }

    private Vector2 ToPosition(int file, int rank) {
        return new Vector2(file, 7 - rank) * SquareSize + new Vector2(Bounds.X, Bounds.Y);
    }

    private int ToRank(Vector2 position) {
        return (int) ((position.X - Bounds.X) / SquareSize);
    }

    private int ToFile(Vector2 position) {
        return 7 - ((int) ((position.Y - Bounds.Y) / SquareSize));
    }

    private bool IsInBoard(Vector2 position) {
        return Bounds.Contains(position);
    }
}
