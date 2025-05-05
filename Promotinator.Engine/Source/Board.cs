using System.Diagnostics;
using Promotinator.Engine.Utils;

namespace Promotinator.Engine;

public class Board {
    public Color Turn;
    public CastlingRights CastlingRights;
    public Coord? EnPassantSquare;
    public Color? ColorOfPlayerInCheck;
    public int FiftyMoveCounter;

    // (file, rank), (0, 0) = a1, (7, 7) = h8.
    public Piece?[,] Pieces = new Piece?[8, 8];

    public Board() {

    }

    public Board(FENBoardState state) {
        Init(state);
    }

    public Board(string fen) {
        SetState(fen);
    }

    public void SetState(string fen) {
        FENBoardState state = FENUtil.ParseFEN(fen);
        Init(state);
    }

    public BoardState MakeMove(Move move) {
        BoardState state = new() {
            EnPassantSquare = EnPassantSquare,
            CastlingRights = CastlingRights,
            FiftyMoveCounter = FiftyMoveCounter
        };

        Debug.Assert(Pieces[move.From.File, move.From.Rank].HasValue, $"Cannot move non-existing piece: {move}");
        Piece piece = Pieces[move.From.File, move.From.Rank].Value;

        // Update 50-move rule
        bool isPieceCaptured = move.CapturedPiece.HasValue;
        bool isPawnMoved = piece.Is(PieceType.Pawn);
        if (!isPieceCaptured && !isPawnMoved) {
            FiftyMoveCounter += 1;
        }
        else {
            FiftyMoveCounter = 0;
        }

        // Check for en passant square
        Coord? newEnPassantSquare = null;
        if (piece.Type == PieceType.Pawn && Math.Abs(move.From.Rank - move.To.Rank) == 2) {
            int rank = Turn == Color.White ? move.From.Rank + 1 : move.From.Rank - 1;
            newEnPassantSquare = new(move.From.File, rank);
        }

        EnPassantSquare = newEnPassantSquare;

        // Update castling rights for when king or rook moved.
        if (Turn == Color.White) {
            if (piece.Is(PieceType.King)) {
                CastlingRights &= ~CastlingRights.WhiteBoth;
            }

            if (piece.Is(PieceType.Rook) && move.From.Rank == 0) {
                if (move.From.File == 0) CastlingRights &= ~CastlingRights.WhiteQueenside;
                if (move.From.File == 7) CastlingRights &= ~CastlingRights.WhiteKingside;
            }
        }
        else {
            if (piece.Is(PieceType.King)) {
                CastlingRights &= ~CastlingRights.BlackBoth;
            }

            if (piece.Is(PieceType.Rook) && move.From.Rank == 7) {
                if (move.From.File == 0) CastlingRights &= ~CastlingRights.BlackQueenside;
                if (move.From.File == 7) CastlingRights &= ~CastlingRights.BlackKingside;
            }
        }

        // Update castling rights for when rook killed.
        if (move.CapturedPiece.HasValue && move.CapturedPiece.Value.Is(PieceType.Rook)) {

            // If we have relevant castling right then check if we need to remove it (if rook is killed).
            if ((CastlingRights & CastlingRights.WhiteQueenside) > 0 && move.To.File == 0 && move.To.Rank == 0) {
                CastlingRights &= ~CastlingRights.WhiteQueenside;
            }

            if ((CastlingRights & CastlingRights.WhiteKingside) > 0 && move.To.File == 7 && move.To.Rank == 0) {
                CastlingRights &= ~CastlingRights.WhiteKingside;
            }

            if ((CastlingRights & CastlingRights.BlackQueenside) > 0 && move.To.File == 0 && move.To.Rank == 7) {
                CastlingRights &= ~CastlingRights.BlackQueenside;
            }

            if ((CastlingRights & CastlingRights.BlackKingside) > 0 && move.To.File == 7 && move.To.Rank == 7) {
                CastlingRights &= ~CastlingRights.BlackKingside;
            }
        }

        Pieces[move.To.File, move.To.Rank] = Pieces[move.From.File, move.From.Rank];
        Pieces[move.From.File, move.From.Rank] = null;

        // Remove captured en passant pawn.
        if (move.IsEnPassantCapture) {
            Pieces[move.To.File, move.From.Rank] = null;
        }

        // Castling
        if (move.IsKingsideCastling) {
            int rank = Turn == Color.White ? 0 : 7;
            Pieces[5, rank] = Pieces[7, rank];
            Pieces[7, rank] = null;
        }
        else if (move.IsQueensideCastling) {
            int rank = Turn == Color.White ? 0 : 7;
            Pieces[3, rank] = Pieces[0, rank];
            Pieces[0, rank] = null;
        }

        // Handle pawn promotion
        if (move.PromotionType != PromotionType.None) {
            Piece p = Pieces[move.To.File, move.To.Rank].Value;
            switch (move.PromotionType) {
                case PromotionType.Queen:
                    p.Type = PieceType.Queen;
                    break;
                case PromotionType.Rook:
                    p.Type = PieceType.Rook;
                    break;
                case PromotionType.Knight:
                    p.Type = PieceType.Knight;
                    break;
                case PromotionType.Bishop:
                    p.Type = PieceType.Bishop;
                    break;
            }

            Pieces[move.To.File, move.To.Rank] = p;
        }

        Turn = Turn == Color.White ? Color.Black : Color.White;
        return state;
    }

    public void UndoMove(Move move, BoardState state) {
        Turn = Turn == Color.White ? Color.Black : Color.White;

        // Place moved piece back
        Pieces[move.From.File, move.From.Rank] = Pieces[move.To.File, move.To.Rank];

        if (move.IsEnPassantCapture) {
            Pieces[move.To.File, move.From.Rank] = move.CapturedPiece;
            Pieces[move.To.File, move.To.Rank] = null;
            EnPassantSquare = new(move.To.File, move.To.Rank);
        }
        else if (move.IsKingsideCastling) {
            int rank = Turn == Color.White ? 0 : 7;
            Pieces[7, rank] = Pieces[5, rank];
            Pieces[5, rank] = null;
            Pieces[move.To.File, move.To.Rank] = null;
        }
        else if (move.IsQueensideCastling) {
            int rank = Turn == Color.White ? 0 : 7;
            Pieces[0, rank] = Pieces[3, rank];
            Pieces[3, rank] = null;
            Pieces[move.To.File, move.To.Rank] = null;
        }
        else {
            Pieces[move.To.File, move.To.Rank] = move.CapturedPiece;

            if (move.PromotionType != PromotionType.None) {
                Piece p = Pieces[move.From.File, move.From.Rank].Value;
                p.Type = PieceType.Pawn;
                Pieces[move.From.File, move.From.Rank] = p;
            }
        }

        EnPassantSquare = state.EnPassantSquare;
        CastlingRights = state.CastlingRights;
        FiftyMoveCounter = state.FiftyMoveCounter;
    }

    public GameState GetState() {
        var moves = MoveGenerator.GenerateMoves(this);

        if (moves.Count == 0) {
            if (IsKingInCheck()) {
                return Turn == Color.White ? GameState.BlackWin : GameState.WhiteWin;
            }

            return GameState.DrawByStalemate;
        }

        if (FiftyMoveCounter >= 100) {
            return GameState.DrawByFiftyMoveRule;
        }

        if (RepitionCount() == 3) {
            return GameState.DrawByThreefoldRepitition;
        }

        if (IsDeadPosition()) {
            return GameState.DrawByDeadPosition;
        }

        return GameState.InProgress;
    }

    public bool IsKingInCheck() {
        Coord kingCoord = FindKingCoord(Turn);
        return IsSquareAttacked(kingCoord);
    }

    private Coord FindKingCoord(Color color) {
        for (int file = 0; file < 8; ++file) {
            for (int rank = 0; rank < 8; ++rank) {
                Piece? piece = Pieces[file, rank];
                if (piece.HasValue && piece.Value.Color == color && piece.Value.Type == PieceType.King) {
                    return new Coord(file, rank);
                }
            }
        }
        // Should never happen in a valid game.
        return new Coord(-1, -1);
    }

    private bool IsSquareAttacked(Coord coord) {
        Color opponent = Turn == Color.White ? Color.Black : Color.White;
        Turn = opponent;

        List<Move> attackingMoves = MoveGenerator.GeneratePseudoLegalMoves(this, onlyAttack: true);

        foreach (Move move in attackingMoves) {
            if (move.To == coord) {
                Turn = opponent == Color.White ? Color.Black : Color.White;
                return true;
            }
        }

        Turn = opponent == Color.White ? Color.Black : Color.White;
        return false;
    }

    private int RepitionCount() {
        return 0;
    }

    private bool IsDeadPosition() {
        return false;
    }

    internal bool IsEmpty(int file, int rank) {
        return !Pieces[file, rank].HasValue;
    }

    internal bool Has(CastlingRights rights) {
        return (CastlingRights & rights) != 0;
    }

    internal bool IsEnemy(int file, int rank) {
        return Pieces[file, rank].HasValue && Pieces[file, rank].Value.Color != Turn;
    }

    internal bool IsFriendly(int file, int rank) {
        return Pieces[file, rank].HasValue && Pieces[file, rank].Value.Color == Turn;
    }

    private void Init(FENBoardState state) {
        Turn = state.Turn;
        CastlingRights = state.CastlingRights;
        EnPassantSquare = state.EnPassantSquare;
        FiftyMoveCounter = 0;
        Pieces = state.Pieces;

        if (IsKingInCheck()) {
            ColorOfPlayerInCheck = Turn;
        }
    }
}
