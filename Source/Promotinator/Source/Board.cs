using System.Diagnostics;
using Promotinator.Engine.Utils;

namespace Promotinator.Engine;

public class Board {
    public Color Turn;
    public CastlingRights CastlingRights;
    public int? EnPassantSquare;
    public Color? ColorOfPlayerInCheck;
    public int FiftyMoveCounter;

    public Piece?[] Pieces = new Piece?[8 * 8];

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

        Debug.Assert(Pieces[move.FromSquare].HasValue, $"Cannot move non-existing piece: {move}");
        Debug.Assert(Pieces[move.FromSquare]!.Value.Color == Turn, $"Wrong player moving: {Turn}");
        Piece piece = Pieces[move.FromSquare]!.Value;

        // Update 50-move rule
        if (!move.IsCapture && !piece.Is(PieceType.Pawn)) {
            FiftyMoveCounter += 1;
        }
        else {
            FiftyMoveCounter = 0;
        }

        // Check for en passant square
        if (move.IsDoublePawnPush) {
            int square = move.FromSquare;
            square += Turn == Color.White ? 8 : -8;
            EnPassantSquare = square;
        }
        else {
            EnPassantSquare = null;
        }

        // Update castling rights for when king or rook moved.
        if (Turn == Color.White) {
            if (piece.Is(PieceType.King)) {
                CastlingRights &= ~CastlingRights.WhiteBoth;
            }

            if (piece.Is(PieceType.Rook) && Move.RankOf(move.FromSquare) == 0) {
                if (Move.FileOf(move.FromSquare) == 0) CastlingRights &= ~CastlingRights.WhiteQueenside;
                if (Move.FileOf(move.FromSquare) == 7) CastlingRights &= ~CastlingRights.WhiteKingside;
            }
        }
        else {
            if (piece.Is(PieceType.King)) {
                CastlingRights &= ~CastlingRights.BlackBoth;
            }

            if (piece.Is(PieceType.Rook) && Move.RankOf(move.FromSquare) == 7) {
                if (Move.FileOf(move.FromSquare) == 0) CastlingRights &= ~CastlingRights.BlackQueenside;
                if (Move.FileOf(move.FromSquare) == 7) CastlingRights &= ~CastlingRights.BlackKingside;
            }
        }

        // Update castling rights for when rook killed.
        if (move.IsCapture && move.CapturedPiece!.Value.Is(PieceType.Rook)) {

            // If we have relevant castling right then check if we need to remove it (if rook is killed).
            if (Has(CastlingRights.WhiteQueenside) && Move.FileOf(move.ToSquare) == 0 && Move.RankOf(move.ToSquare) == 0) {
                CastlingRights &= ~CastlingRights.WhiteQueenside;
            }

            if (Has(CastlingRights.WhiteKingside) && Move.FileOf(move.ToSquare) == 7 && Move.RankOf(move.ToSquare) == 0) {
                CastlingRights &= ~CastlingRights.WhiteKingside;
            }

            if (Has(CastlingRights.BlackQueenside) && Move.FileOf(move.ToSquare) == 0 && Move.RankOf(move.ToSquare) == 7) {
                CastlingRights &= ~CastlingRights.BlackQueenside;
            }

            if (Has(CastlingRights.BlackKingside) && Move.FileOf(move.ToSquare) == 7 && Move.RankOf(move.ToSquare) == 7) {
                CastlingRights &= ~CastlingRights.BlackKingside;
            }
        }

        Pieces[move.ToSquare] = Pieces[move.FromSquare];
        Pieces[move.FromSquare] = null;

        if (!move.IsQuietMove) {
            // Remove captured en passant pawn.
            if (move.IsEnPassantCapture) {
                int file = Move.FileOf(move.ToSquare);
                int rank = Move.RankOf(move.FromSquare);
                int index = Move.Index(file, rank);
                Pieces[index] = null;
            }

            // Castling
            if (move.IsKingCastle) {
                int rank = Turn == Color.White ? 0 : 7;
                Pieces[Move.Index(5, rank)] = Pieces[Move.Index(7, rank)];
                Pieces[Move.Index(7, rank)] = null;
            }
            else if (move.IsQueenCastle) {
                int rank = Turn == Color.White ? 0 : 7;
                Pieces[Move.Index(3, rank)] = Pieces[Move.Index(0, rank)];
                Pieces[Move.Index(0, rank)] = null;
            }

            // Handle pawn promotion
            if (move.IsPromotion) {
                Piece p = Pieces[move.ToSquare]!.Value;

                if (move.IsKnightPromotion) p.Type = PieceType.Knight;
                else if (move.IsBishopPromotion) p.Type = PieceType.Bishop;
                else if (move.IsRookPromotion) p.Type = PieceType.Rook;
                else if (move.IsQueenPromotion) p.Type = PieceType.Queen;
                else Debug.Assert(false);

                Pieces[move.ToSquare] = p;
            }
        }

        Turn = Turn == Color.White ? Color.Black : Color.White;
        return state;
    }

    public void UndoMove(Move move, BoardState state) {
        Turn = Turn == Color.White ? Color.Black : Color.White;

        // Place moved piece back
        Pieces[move.FromSquare] = Pieces[move.ToSquare];

        if (move.IsEnPassantCapture) {
            int file = Move.FileOf(move.ToSquare);
            int rank = Move.RankOf(move.FromSquare);
            int index = Move.Index(file, rank);
            Pieces[index] = move.CapturedPiece;
            Pieces[move.ToSquare] = null;
            EnPassantSquare = move.ToSquare;
        }
        else if (move.IsKingCastle) {
            int rank = Turn == Color.White ? 0 : 7;
            Pieces[Move.Index(7, rank)] = Pieces[Move.Index(5, rank)];
            Pieces[Move.Index(5, rank)] = null;
            Pieces[move.ToSquare] = null;
        }
        else if (move.IsQueenCastle) {
            int rank = Turn == Color.White ? 0 : 7;
            Pieces[Move.Index(0, rank)] = Pieces[Move.Index(3, rank)];
            Pieces[Move.Index(3, rank)] = null;
            Pieces[move.ToSquare] = null;
        }
        else {
            Pieces[move.ToSquare] = move.CapturedPiece;

            if (move.IsPromotion) {
                Piece p = Pieces[move.FromSquare]!.Value;
                p.Type = PieceType.Pawn;
                Pieces[move.FromSquare] = p;
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
        int kingSquare = FindKingSquare(Turn);
        return IsSquareAttacked(kingSquare);
    }

    public bool IsEmpty(int file, int rank) {
        return !Pieces[Move.Index(file, rank)].HasValue;
    }

    public bool Has(CastlingRights rights) {
        return (CastlingRights & rights) != 0;
    }

    public bool IsEnemy(int square) {
        return Pieces[square].HasValue && Pieces[square]!.Value.Color != Turn;
    }

    public bool IsFriendly(int square) {
        return Pieces[square].HasValue && Pieces[square]!.Value.Color == Turn;
    }

    private int FindKingSquare(Color color) {
        for (int i = 0; i < 64; ++i) {
            Piece? piece = Pieces[i];
            if (piece.HasValue && piece.Value.Color == color && piece.Value.Type == PieceType.King) {
                return i;
            }
        }

        // Should never happen in a valid game.
        Debug.Assert(false);
        return -1;
    }

    private bool IsSquareAttacked(int square) {
        Color opponent = Turn == Color.White ? Color.Black : Color.White;
        Turn = opponent;

        List<Move> attackingMoves = MoveGenerator.GeneratePseudoLegalMoves(this, onlyAttack: true);

        foreach (Move move in attackingMoves) {
            if (move.ToSquare == square) {
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

    public string ToFEN() {
        string[] rows = new string[8];
        for (int rank = 7; rank >= 0; rank--) {
            string row = "";
            int empty = 0;
            for (int file = 0; file < 8; file++) {
                Piece? piece = Pieces[Move.Index(file, rank)];
                if (!piece.HasValue) {
                    empty++;
                }
                else {
                    if (empty > 0) {
                        row += empty.ToString();
                        empty = 0;
                    }
                    char symbol = piece.Value.Type switch {
                        PieceType.Pawn => 'P',
                        PieceType.Knight => 'N',
                        PieceType.Bishop => 'B',
                        PieceType.Rook => 'R',
                        PieceType.Queen => 'Q',
                        PieceType.King => 'K',
                        _ => '?'
                    };
                    row += piece.Value.Color == Color.White ? symbol : char.ToLower(symbol);
                }
            }
            if (empty > 0) row += empty.ToString();
            rows[7 - rank] = row;
        }

        string piecePlacement = string.Join("/", rows);
        string activeColor = Turn == Color.White ? "w" : "b";

        string castling = "";
        if ((CastlingRights & CastlingRights.WhiteKingside) != 0) castling += "K";
        if ((CastlingRights & CastlingRights.WhiteQueenside) != 0) castling += "Q";
        if ((CastlingRights & CastlingRights.BlackKingside) != 0) castling += "k";
        if ((CastlingRights & CastlingRights.BlackQueenside) != 0) castling += "q";
        if (castling == "") castling = "-";

        string enPassant = string.Empty;
        if (EnPassantSquare.HasValue) {
            int file = Move.FileOf(EnPassantSquare.Value);
            int rank = Move.RankOf(EnPassantSquare.Value);
            enPassant = $"{(char)('a' + file)}{rank + 1}";
        }

        return $"{piecePlacement} {activeColor} {castling} {enPassant} {FiftyMoveCounter} 1";
    }
}
