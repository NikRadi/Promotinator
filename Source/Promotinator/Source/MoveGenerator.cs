using System.Diagnostics;

namespace Promotinator.Engine;

public static class MoveGenerator {
    public static List<Move> GenerateMoves(Board board) {
        List<Move> moves = GeneratePseudoLegalMoves(board);

        Debug.Assert(moves.Count > 0, $"GenerateMoves: found no moves");
        Debug.Assert(moves.All(move => board.Pieces[move.From.File, move.From.Rank].HasValue));

        int moveIndex = moves.FindIndex(m => m.CapturedPiece.HasValue && m.CapturedPiece.Value.Type == PieceType.King);
        if (moveIndex > 0) {
            board.ColorOfPlayerInCheck = moves[moveIndex].CapturedPiece.Value.Color;
        }
        else {
            board.ColorOfPlayerInCheck = null;
        }

        moves.RemoveAll(move => !IsLegalMove(board, move));
        return moves;
    }

    public static List<Move> GeneratePseudoLegalMoves(Board board, bool onlyAttack = false) {
        List<Move> moves = [];

        for (int file = 0; file < 8; ++file) {
            for (int rank = 0; rank < 8; ++rank) {
                Piece? piece = board.Pieces[file, rank];

                if (piece.HasValue && piece.Value.Color == board.Turn) {
                    switch (piece.Value.Type) {
                        case PieceType.Pawn:
                            GeneratePawnMoves(board, moves, file, rank, onlyAttack);
                            break;
                        case PieceType.Bishop:
                            GenerateBishopMoves(board, moves, file, rank, onlyAttack);
                            break;
                        case PieceType.Knight:
                            GenerateKnightMoves(board, moves, file, rank, onlyAttack);
                            break;
                        case PieceType.Rook:
                            GenerateRookMoves(board, moves, file, rank, onlyAttack);
                            break;
                        case PieceType.Queen:
                            GenerateBishopMoves(board, moves, file, rank, onlyAttack);
                            GenerateRookMoves(board, moves, file, rank, onlyAttack);
                            break;
                        case PieceType.King:
                            if (!onlyAttack) {
                                GenerateKingMoves(board, moves, file, rank, onlyAttack);
                            }

                            break;
                    }
                }
            }
        }

        return moves;
    }

    private static void GeneratePawnMoves(Board board, List<Move> moves, int file, int rank, bool onlyAttack = false) {
        int startRank = board.Turn == Color.White ? 1 : 6;
        int direction = board.Turn == Color.White ? 1 : -1;
        int forward = rank + direction;
        bool isPromotion = forward == 0 || forward == 7;

        // One square forward
        if (!onlyAttack && board.IsEmpty(file, forward)) {
            Move move = new() {
                From = new(file, rank),
                To = new(file, forward)
            };

            if (isPromotion) {
                AddPawnPromotionMoves(moves, move);
            }
            else {
                moves.Add(move);
            }
        }

        // Two squares forward
        if (!onlyAttack && rank == startRank && board.IsEmpty(file, forward) && board.IsEmpty(file, forward + direction)) {
            moves.Add(new() {
                From = new(file, rank),
                To = new(file, forward + direction)
            });
        }

        // Capture left
        if (file > 0 && (board.IsEnemy(file - 1, forward) || onlyAttack)) {
            Move move =new() {
                From = new(file, rank),
                To = new(file - 1, forward),
                CapturedPiece = board.Pieces[file - 1, forward]
            };

            if (isPromotion) {
                AddPawnPromotionMoves(moves, move);
            }
            else {
                moves.Add(move);
            }
        }

        // Capture right
        if (file < 7 && (board.IsEnemy(file + 1, forward) || onlyAttack)) {
            Move move = new() {
                From = new(file, rank),
                To = new(file + 1, forward),
                CapturedPiece = board.Pieces[file + 1, forward]
            };

            if (isPromotion) {
                AddPawnPromotionMoves(moves, move);
            }
            else {
                moves.Add(move);
            }
        }

        // En passant
        if (board.EnPassantSquare.HasValue && forward == board.EnPassantSquare.Value.Rank) {
            // Capture left
            if ((file - 1) == board.EnPassantSquare.Value.File) {
                moves.Add(new() {
                    From = new(file, rank),
                    To = new(file - 1, forward),
                    CapturedPiece = board.Pieces[file - 1, rank],
                    IsEnPassantCapture = true
                });
            }

            // Capture right
            if ((file + 1) == board.EnPassantSquare.Value.File) {
                moves.Add(new() {
                    From = new(file, rank),
                    To = new(file + 1, forward),
                    CapturedPiece = board.Pieces[file + 1, rank],
                    IsEnPassantCapture = true
                });
            }
        }
    }

    private static void GenerateBishopMoves(Board board, List<Move> moves, int file, int rank, bool onlyAttack = false) {
        int[] rankOffsets = [-1, -1, 1, 1];
        int[] fileOffsets = [-1, 1, -1, 1];

        for (int i = 0; i < 4; ++i) {
            int newFile = file + fileOffsets[i];
            int newRank = rank + rankOffsets[i];

            while (newFile >= 0 && newFile < 8 && newRank >= 0 && newRank < 8) {
                if (board.IsEmpty(newFile, newRank)) {
                    moves.Add(new() {
                        From = new(file, rank),
                        To = new(newFile, newRank)
                    });
                }
                else if (board.IsEnemy(newFile, newRank)) {
                    moves.Add(new() {
                        From = new(file, rank),
                        To = new(newFile, newRank),
                        CapturedPiece = board.Pieces[newFile, newRank]
                    });

                    break;
                }
                else {
                    // Blocked by friendly piece
                    break;
                }

                newRank += rankOffsets[i];
                newFile += fileOffsets[i];
            }
        }
    }

    private static void GenerateKnightMoves(Board board, List<Move> moves, int file, int rank, bool onlyAttack = false) {
        int[] rankOffsets = [-2, -2, -1, -1, 1, 1, 2, 2];
        int[] fileOffsets = [-1, 1, -2, 2, -2, 2, -1, 1];

        for (int i = 0; i < 8; ++i) {
            int newFile = file + fileOffsets[i];
            int newRank = rank + rankOffsets[i];

            if (newFile >= 0 && newFile < 8 && newRank >= 0 && newRank < 8) {
                if (board.IsEmpty(newFile, newRank)) {
                    moves.Add(new() {
                        From = new(file, rank),
                        To = new(newFile, newRank),
                    });
                }

                if (board.IsEnemy(newFile, newRank)) {
                    moves.Add(new() {
                        From = new(file, rank),
                        To = new(newFile, newRank),
                        CapturedPiece = board.Pieces[newFile, newRank]
                    });
                }
            }
        }
    }

    private static void GenerateRookMoves(Board board, List<Move> moves, int file, int rank, bool onlyAttack = false) {
        int[] rankOffsets = [-1, 0, 1, 0];
        int[] fileOffsets = [0, 1, 0, -1];

        for (int i = 0; i < 4; ++i) {
            int newFile = file + fileOffsets[i];
            int newRank = rank + rankOffsets[i];

            while (newFile >= 0 && newFile < 8 && newRank >= 0 && newRank < 8) {
                if (board.IsEmpty(newFile, newRank)) {
                    moves.Add(new() {
                        From = new(file, rank),
                        To = new(newFile, newRank)
                    });
                }
                else if (board.IsEnemy(newFile, newRank)) {
                    moves.Add(new() {
                        From = new(file, rank),
                        To = new(newFile, newRank),
                        CapturedPiece = board.Pieces[newFile, newRank]
                    });

                    break;
                }
                else {
                    // Blocked by friendly piece
                    break;
                }

                newRank += rankOffsets[i];
                newFile += fileOffsets[i];
            }
        }
    }

    private static void GenerateKingMoves(Board board, List<Move> moves, int file, int rank, bool onlyAttack = false) {
        int[] rankOffsets = [-1, -1, -1, 0, 0, 1, 1, 1];
        int[] fileOffsets = [-1, 0, 1, -1, 1, -1, 0, 1];

        for (int i = 0; i < 8; ++i) {
            int newFile = file + fileOffsets[i];
            int newRank = rank + rankOffsets[i];

            if (newFile >= 0 && newFile < 8 && newRank >= 0 && newRank < 8) {
                if (!onlyAttack && board.IsEmpty(newFile, newRank)) {
                    moves.Add(new() {
                        From = new(file, rank),
                        To = new(newFile, newRank),
                    });
                }

                if (board.IsEnemy(newFile, newRank)) {
                    moves.Add(new() {
                        From = new(file, rank),
                        To = new(newFile, newRank),
                        CapturedPiece = board.Pieces[newFile, newRank]
                    });
                }
            }
        }

        // Castling
        if (!onlyAttack) {
            if (board.Turn == Color.White) {
                if (board.Has(CastlingRights.WhiteKingside) && board.IsEmpty(5, 0) && board.IsEmpty(6, 0) && board.ColorOfPlayerInCheck != board.Turn) {
                    board.Turn = board.Turn == Color.White ? Color.Black : Color.White;
                    List<Move> enemyMoves = GeneratePseudoLegalMoves(board, onlyAttack: true);
                    board.Turn = board.Turn == Color.White ? Color.Black : Color.White;

                    // Check if opponent can attack square (5, 0) or (6, 0).
                    if (!enemyMoves.Exists(move => (move.To.File == 4 || move.To.File == 5 || move.To.File == 6) && move.To.Rank == 0)) {
                        moves.Add(new() {
                            From = new(4, 0),
                            To = new(6, 0),
                            IsKingsideCastling = true
                        });
                    }
                }

                if (board.Has(CastlingRights.WhiteQueenside) && board.IsEmpty(3, 0) && board.IsEmpty(2, 0) && board.IsEmpty(1, 0) && board.ColorOfPlayerInCheck != board.Turn) {
                    board.Turn = board.Turn == Color.White ? Color.Black : Color.White;
                    List<Move> enemyMoves = GeneratePseudoLegalMoves(board, onlyAttack: true);
                    board.Turn = board.Turn == Color.White ? Color.Black : Color.White;

                    // Check if opponent can attack square (3, 0) or (2, 0).
                    if (!enemyMoves.Exists(move => (move.To.File == 4 || move.To.File == 3 || move.To.File == 2) && move.To.Rank == 0)) {
                        moves.Add(new() {
                            From = new(4, 0),
                            To = new(2, 0),
                            IsQueensideCastling = true
                        });
                    }
                }
            }
            else {
                if (board.Has(CastlingRights.BlackKingside) && board.IsEmpty(5, 7) && board.IsEmpty(6, 7) && board.ColorOfPlayerInCheck != board.Turn) {
                    board.Turn = board.Turn == Color.White ? Color.Black : Color.White;
                    List<Move> enemyMoves = GeneratePseudoLegalMoves(board, onlyAttack: true);
                    board.Turn = board.Turn == Color.White ? Color.Black : Color.White;

                    // Check if opponent can attack square (5, 7) or (6, 7).
                    if (!enemyMoves.Exists(move => (move.To.File == 4 || move.To.File == 5 || move.To.File == 6) && move.To.Rank == 7)) {
                        moves.Add(new() {
                            From = new(4, 7),
                            To = new(6, 7),
                            IsKingsideCastling = true
                        });
                    }
                }

                if (board.Has(CastlingRights.BlackQueenside) && board.IsEmpty(3, 7) && board.IsEmpty(2, 7) && board.IsEmpty(1, 7) && board.ColorOfPlayerInCheck != board.Turn) {
                    board.Turn = board.Turn == Color.White ? Color.Black : Color.White;
                    List<Move> enemyMoves = GeneratePseudoLegalMoves(board, onlyAttack: true);
                    board.Turn = board.Turn == Color.White ? Color.Black : Color.White;

                    // Check if opponent can attack square (3, 7) or (2, 7).
                    if (!enemyMoves.Exists(move => (move.To.File == 4 ||move.To.File == 3 || move.To.File == 2) && move.To.Rank == 7)) {
                        moves.Add(new() {
                            From = new(4, 7),
                            To = new(2, 7),
                            IsQueensideCastling = true
                        });
                    }
                }
            }
        }
    }

    private static bool IsLegalMove(Board board, Move move) {
        bool isLegalMove = true;
        BoardState state = board.MakeMove(move);

        List<Move> moves = GeneratePseudoLegalMoves(board);
        foreach (Move m in moves) {
            if (m.CapturedPiece.HasValue && m.CapturedPiece.Value.Type == PieceType.King) {
                isLegalMove = false;
                break;
            }
        }

        board.UndoMove(move, state);
        return isLegalMove;
    }

    private static void AddPawnPromotionMoves(List<Move> moves, Move move) {
        move.PromotionType = PromotionType.Queen;
        moves.Add(move);

        move.PromotionType = PromotionType.Rook;
        moves.Add(move);

        move.PromotionType = PromotionType.Knight;
        moves.Add(move);

        move.PromotionType = PromotionType.Bishop;
        moves.Add(move);
    }
}
