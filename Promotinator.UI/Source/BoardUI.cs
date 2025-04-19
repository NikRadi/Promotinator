using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Promotinator.UI.Players;

namespace Promotinator.UI;

public class BoardUI {
    public bool AreBlackPiecesLocked;
    public bool AreWhitePiecesLocked;

    // (file, rank), (0, 0) = a1, (7, 7) = h8.
    private Piece[,] Pieces { get; } = new Piece[8, 8];
    private Square[,] Squares { get; } = new Square[8, 8];

    private RectangleUI[] Borders { get; } = new RectangleUI[4];
    private Color BorderColor { get; } = new(100, 100, 100);

    private int SquareSize { get; }
    private int BorderSize { get; }
    private Rectangle Bounds { get; }
    private Piece _draggedPiece;
    private Coord _draggedFrom;
    private GameController _controller;
    private bool _isWhiteBottom = true;

    public BoardUI(GameController controller, Vector2 position, int size) {
        _controller = controller;

        SquareSize = size / 8;
        BorderSize = size / 60;
        Bounds = new((int) position.X, (int) position.Y, size, size);

        CreateBorders();
        CreateSquares();
    }

    public void Update() {
        if (Input.IsLeftMouseButtonDown()) {
            HandleMouseDown();
        }

        if (Input.IsLeftMouseButtonReleased()) {
            HandleMouseRelease();
        }

        if (Input.IsKeyPressedOnce(Keys.Back)) {
//            UndoLastMove();
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

    public void PlacePieces(Engine.Board board) {
        for (int file = 0; file < 8; ++file) {
            for (int rank = 0; rank < 8; ++rank) {
                Engine.Piece? piece = board.Pieces[file, rank];
                if (piece.HasValue) {
                    PlacePiece(board.Pieces[file, rank].Value, file, rank);
                }
                else {
                    Pieces[file, rank] = null;
                }
            }
        }
    }

    public void SetHighlightedSquares(List<Coord> squares, bool isHighlighted) {
        foreach (Coord square in squares) {
            Squares[square.File, square.Rank].IsHighlighted = isHighlighted;
        }
    }

    public void SetMoveHighlight(Coord from, Coord to, bool isHighlighted) {
        Squares[from.File, from.Rank].WasLastMove = isHighlighted;
        Squares[to.File, to.Rank].WasLastMove = isHighlighted;
    }

    public void ResetDraggedPiece() {
        _draggedPiece.Position = ToPosition(_draggedFrom.File, _draggedFrom.Rank);
        _draggedPiece = null;
    }

    public void SetPerspective(bool isWhiteBottom) {
        _isWhiteBottom = isWhiteBottom;
        ResetSquarePositions();
    }

    private void ResetSquarePositions() {
        for (int file = 0; file < 8; ++file) {
            for (int rank = 0; rank < 8; ++rank) {
                Squares[file, rank].Position = ToPosition(file, rank);

                if (Pieces[file, rank] != null) {
                    Pieces[file, rank].Position = ToPosition(file, rank);
                }
            }
        }
    }

    private void HandleMouseDown() {
        if (!IsInBoard(Input.MousePosition)) {
            return;
        }

        Coord coord = ToCoord(Input.MousePosition);
        Piece piece = Pieces[coord.File, coord.Rank];
        if (piece != null && ((piece.Color == PieceColor.White && AreWhitePiecesLocked) || (piece.Color == PieceColor.Black && AreBlackPiecesLocked))) {
            return;
        }

        _draggedFrom = coord;
        _draggedPiece = piece;
        _controller.OnMouseDown(coord);
    }

    private void HandleMouseRelease() {
        if (_draggedPiece == null) {
            return;
        }

        Coord coord = ToCoord(Input.MousePosition);
        _controller.OnMouseRelease(_draggedFrom, coord);
        _draggedPiece.Position = ToPosition(_draggedFrom.File, _draggedFrom.Rank);
        _draggedPiece = null;
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

    private Coord ToCoord(Vector2 position) {
        int file = (int) ((position.X - Bounds.X) / SquareSize);
        int rank = (int) ((position.Y - Bounds.Y) / SquareSize);
        if (_isWhiteBottom) {
            rank = 7 - rank;
        }

        return new(file, rank);
    }

    private Vector2 ToPosition(int file, int rank) {
        int r = _isWhiteBottom ? 7 - rank : rank;
        return new Vector2(file, r) * SquareSize + new Vector2(Bounds.X, Bounds.Y);
    }

    private bool IsInBoard(Vector2 position) {
        return Bounds.Contains(position);
    }
}
