using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Promotinator.Graphic;
using Promotinator.Graphics.Util;

namespace Promotinator.Graphics.UI;

public class BoardUI {
    public event EventHandler<Coord> OnPieceDragStarted;
    public event EventHandler<Move> OnPieceDragEnded;

    public bool AreBlackPiecesLocked;
    public bool AreWhitePiecesLocked;

    // (file, rank), (0, 0) = a1, (7, 7) = h8.
    private readonly PieceUI[,] Pieces = new PieceUI[8, 8];
    private readonly SquareUI[,] Squares = new SquareUI[8, 8];

    private readonly int SquareSize;
    private readonly Rectangle Bounds;
    private readonly RectangleUI[] Borders = new RectangleUI[4];

    private PieceUI _draggedPiece;
    private Coord _draggedFrom;
    private bool _isWhiteBottom = true;
    private Coord[] _lastMove = [Coord.Null, Coord.Null];

    public BoardUI(Vector2 position, int size) {
        SquareSize = size / 8;
        Bounds = new((int) position.X, (int) position.Y, size, size);

        CreateBorders();
        CreateSquares();
        AddLabels();
    }

    public void Update() {
        if (Input.IsLeftMouseButtonDown()) {
            HandleMouseDown();
        }

        if (Input.IsLeftMouseButtonReleased()) {
            HandleMouseUp();
        }

        if (_draggedPiece != null) {
            _draggedPiece.Position = Input.MousePosition - (new Vector2(SquareSize) / 2);
        }
    }

    public void Draw(SpriteBatch spriteBatch) {
        foreach (var border in Borders) {
            border.Draw(spriteBatch);
        }

        for (int file = 0; file < 8; ++file) {
            for (int rank = 0; rank < 8; ++rank) {
                Squares[file, rank].Draw(spriteBatch);

                var p = Pieces[file, rank];
                if (p != null && p != _draggedPiece) {
                    p.Draw(spriteBatch);
                }
            }
        }

        // Drawn last to appear on top of everything else.
        _draggedPiece?.Draw(spriteBatch);
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

    public void SetPotentialMoveHighlight(Coord square, bool isHighlighted) {
        Squares[square.File, square.Rank].SetPotentialMoveHighlight(isHighlighted);
    }

    public void SetLastMoveHighlight(Coord from, Coord to, bool isHighlighted) {
        Squares[from.File, from.Rank].SetLastMoveHighlight(isHighlighted);
        Squares[to.File, to.Rank].SetLastMoveHighlight(isHighlighted);
    }

    public void SetPerspective(bool isWhiteBottom) {
        _isWhiteBottom = isWhiteBottom;
        ResetSquarePositions();
    }

    public void SetLastMove(Coord from, Coord to) {
        if (_lastMove[0] != Coord.Null && _lastMove[1] != Coord.Null) {
            SetLastMoveHighlight(_lastMove[0], _lastMove[1], isHighlighted: false);
        }

        SetLastMoveHighlight(from, to, isHighlighted: true);
        _lastMove[0] = from;
        _lastMove[1] = to;
    }


    //
    // Events
    //


    private void HandleMouseDown() {
        if (!IsInBoard(Input.MousePosition)) {
            return;
        }

        Coord coord = ToCoord(Input.MousePosition);
        var piece = Pieces[coord.File, coord.Rank];

        if (piece == null || IsColorLocked(piece.Color)) {
            return;
        }

        _draggedPiece = piece;
        _draggedFrom = coord;
        OnPieceDragStarted?.Invoke(this, coord);
    }

    private void HandleMouseUp() {
        if (_draggedPiece == null) {
            return;
        }

        _draggedPiece.Position = ToPosition(_draggedFrom.File, _draggedFrom.Rank);
        _draggedPiece = null;

        if (!IsInBoard(Input.MousePosition)) {
            OnPieceDragEnded?.Invoke(this, Move.Null);
            return;
        }

        Coord draggedTo = ToCoord(Input.MousePosition);
        OnPieceDragEnded?.Invoke(this, new(_draggedFrom, draggedTo));
    }


    //
    // Drawing
    //


    private void PlacePiece(Engine.Piece piece, int file, int rank) {
        PlayerColor color;
        if (piece.Color == Engine.Color.Black) color = PlayerColor.Black;
        else if (piece.Color == Engine.Color.White) color = PlayerColor.White;
        else throw new ArgumentException($"Invalid piece color: {piece.Color}");

        PieceType type;
        if (piece.Type == Engine.PieceType.King) type = PieceType.King;
        else if (piece.Type == Engine.PieceType.Queen) type = PieceType.Queen;
        else if (piece.Type == Engine.PieceType.Rook) type = PieceType.Rook;
        else if (piece.Type == Engine.PieceType.Bishop) type = PieceType.Bishop;
        else if (piece.Type == Engine.PieceType.Knight) type = PieceType.Knight;
        else if (piece.Type == Engine.PieceType.Pawn) type = PieceType.Pawn;
        else throw new ArgumentException($"Invalid piece type: {piece.Type}");

        Pieces[file, rank] = new(
            color: color,
            type: type,
            position: ToPosition(file, rank),
            size: SquareSize
        );
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

    private void CreateBorders() {
        Color color = new(100, 100, 100);
        int size = SquareSize / 3;

        // Top
        Borders[0] = new(
            position: new(Bounds.Left - size, Bounds.Top - size),
            size: new(Bounds.Width + size * 2, size),
            color: color
        );

        // Bottom
        Borders[1] = new(
            position: new(Bounds.Left - size, Bounds.Bottom),
            size: new(Bounds.Width + size * 2, size),
            color: color
        );

        // Left
        Borders[2] = new(
            position: new(Bounds.Left - size, Bounds.Top),
            size: new(size, Bounds.Height),
            color: color
        );

        // Right
        Borders[3] = new(
            position: new(Bounds.Right, Bounds.Top),
            size: new(size, Bounds.Height),
            color: color
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

    private void AddLabels() {
        // TODO: Replace with SpriteFont.MeasureString() to center correctly.
        int rankOffsetX = -15;

        for (int rank = 0; rank < 8; ++rank) {
            Text text = new() {
                Position = ToPosition(0, rank) + new Vector2(rankOffsetX, 0),
                Value = (rank + 1).ToString()
            };

            TextRenderer.Add(text);
        }

        for (int file = 0; file < 8; ++file) {
            Text text = new() {
                Position = ToPosition(file, 0) + new Vector2(0, SquareSize),
                Value = ((char) (file + 'a')).ToString()
            };

            TextRenderer.Add(text);
        }
    }


    //
    // Utility Functions
    //


    private bool IsInBoard(Vector2 position) {
        return Bounds.Contains(position);
    }

    private bool IsColorLocked(PlayerColor color) {
        return (color == PlayerColor.White && AreWhitePiecesLocked) || (color == PlayerColor.Black && AreBlackPiecesLocked);
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
}
