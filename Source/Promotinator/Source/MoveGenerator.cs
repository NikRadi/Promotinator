using System.Diagnostics;

namespace Promotinator.Engine;

public static class MoveGenerator {
    public static List<Move> GenerateMoves(Board board) {
        List<Move> moves = GeneratePseudoLegalMoves(board);

        Debug.Assert(moves.Count > 0, $"GenerateMoves: found no moves");
        Debug.Assert(moves.All(move => board.Pieces[move.FromIdx].HasValue));

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
                Piece? piece = board.Pieces[Move.Index(file, rank)];

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
        Debug.Assert(0 < rank && rank < 7);
        int startRank = board.Turn == Color.White ? 1 : 6;
        int direction = board.Turn == Color.White ? 1 : -1;
        int forward = rank + direction;
        bool isPromotion = forward == 0 || forward == 7;

        // One square forward
        if (!onlyAttack && board.IsEmpty(file, forward)) {
            int from = Move.Index(file, rank);
            int to = Move.Index(file, forward);
            Move move = new(from, to, Move.QuietMoveFlag) {
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

        // Double pawn push
        if (!onlyAttack && rank == startRank && board.IsEmpty(file, forward) && board.IsEmpty(file, forward + direction)) {
            int from = Move.Index(file, rank);
            int to = Move.Index(file, forward + direction);
            moves.Add(new(from, to, Move.DoublePawnPushFlag) {
                From = new(file, rank),
                To = new(file, forward + direction)
            });
        }

        // Capture left
        if (file > 0 && (board.IsEnemy(file - 1, forward) || onlyAttack)) {
            int from = Move.Index(file, rank);
            int to = Move.Index(file - 1, forward);
            Move move = new(from, to, Move.CaptureFlag) {
                From = new(file, rank),
                To = new(file - 1, forward),
                CapturedPiece = board.Pieces[Move.Index(file - 1, forward)]
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
            int from = Move.Index(file, rank);
            int to = Move.Index(file + 1, forward);
            Move move = new(from, to, Move.CaptureFlag) {
                From = new(file, rank),
                To = new(file + 1, forward),
                CapturedPiece = board.Pieces[Move.Index(file + 1, forward)]
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
                int from = Move.Index(file, rank);
                int to = Move.Index(file - 1, forward);
                moves.Add(new(from, to, Move.EnPassantCaptureFlag) {
                    From = new(file, rank),
                    To = new(file - 1, forward),
                    CapturedPiece = board.Pieces[Move.Index(file - 1, rank)],
                });
            }

            // Capture right
            if ((file + 1) == board.EnPassantSquare.Value.File) {
                int from = Move.Index(file, rank);
                int to = Move.Index(file + 1, forward);
                moves.Add(new(from, to, Move.EnPassantCaptureFlag) {
                    From = new(file, rank),
                    To = new(file + 1, forward),
                    CapturedPiece = board.Pieces[Move.Index(file + 1, rank)],
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
                    int from = Move.Index(file, rank);
                    int to = Move.Index(newFile, newRank);
                    moves.Add(new(from, to, Move.QuietMoveFlag) {
                        From = new(file, rank),
                        To = new(newFile, newRank)
                    });
                }
                else if (board.IsEnemy(newFile, newRank)) {
                    int from = Move.Index(file, rank);
                    int to = Move.Index(newFile, newRank);
                    moves.Add(new(from, to, Move.CaptureFlag) {
                        From = new(file, rank),
                        To = new(newFile, newRank),
                        CapturedPiece = board.Pieces[Move.Index(newFile, newRank)]
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
                    int from = Move.Index(file, rank);
                    int to = Move.Index(newFile, newRank);
                    moves.Add(new(from, to, Move.QuietMoveFlag) {
                        From = new(file, rank),
                        To = new(newFile, newRank),
                    });
                }

                if (board.IsEnemy(newFile, newRank)) {
                    int from = Move.Index(file, rank);
                    int to = Move.Index(newFile, newRank);
                    moves.Add(new(from, to, Move.CaptureFlag) {
                        From = new(file, rank),
                        To = new(newFile, newRank),
                        CapturedPiece = board.Pieces[Move.Index(newFile, newRank)]
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
                    int from = Move.Index(file, rank);
                    int to = Move.Index(newFile, newRank);
                    moves.Add(new(from, to, Move.QuietMoveFlag) {
                        From = new(file, rank),
                        To = new(newFile, newRank)
                    });
                }
                else if (board.IsEnemy(newFile, newRank)) {
                    int from = Move.Index(file, rank);
                    int to = Move.Index(newFile, newRank);
                    moves.Add(new(from, to, Move.CaptureFlag) {
                        From = new(file, rank),
                        To = new(newFile, newRank),
                        CapturedPiece = board.Pieces[Move.Index(newFile, newRank)]
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
                    int from = Move.Index(file, rank);
                    int to = Move.Index(newFile, newRank);
                    moves.Add(new(from, to, Move.QuietMoveFlag) {
                        From = new(file, rank),
                        To = new(newFile, newRank),
                    });
                }

                if (board.IsEnemy(newFile, newRank)) {
                    int from = Move.Index(file, rank);
                    int to = Move.Index(newFile, newRank);
                    moves.Add(new(from, to, Move.CaptureFlag) {
                        From = new(file, rank),
                        To = new(newFile, newRank),
                        CapturedPiece = board.Pieces[Move.Index(newFile, newRank)]
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
                        int from = Move.Index(4, 0);
                        int to = Move.Index(6, 0);
                        moves.Add(new(from, to, Move.KingCastleFlag) {
                            From = new(4, 0),
                            To = new(6, 0),
                        });
                    }
                }

                if (board.Has(CastlingRights.WhiteQueenside) && board.IsEmpty(3, 0) && board.IsEmpty(2, 0) && board.IsEmpty(1, 0) && board.ColorOfPlayerInCheck != board.Turn) {
                    board.Turn = board.Turn == Color.White ? Color.Black : Color.White;
                    List<Move> enemyMoves = GeneratePseudoLegalMoves(board, onlyAttack: true);
                    board.Turn = board.Turn == Color.White ? Color.Black : Color.White;

                    // Check if opponent can attack square (3, 0) or (2, 0).
                    if (!enemyMoves.Exists(move => (move.To.File == 4 || move.To.File == 3 || move.To.File == 2) && move.To.Rank == 0)) {
                        int from = Move.Index(4, 0);
                        int to = Move.Index(2, 0);
                        moves.Add(new(from, to, Move.QueenCastleFlag) {
                            From = new(4, 0),
                            To = new(2, 0),
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
                        int from = Move.Index(4, 7);
                        int to = Move.Index(6, 7);
                        moves.Add(new(from, to, Move.KingCastleFlag) {
                            From = new(4, 7),
                            To = new(6, 7),
                        });
                    }
                }

                if (board.Has(CastlingRights.BlackQueenside) && board.IsEmpty(3, 7) && board.IsEmpty(2, 7) && board.IsEmpty(1, 7) && board.ColorOfPlayerInCheck != board.Turn) {
                    board.Turn = board.Turn == Color.White ? Color.Black : Color.White;
                    List<Move> enemyMoves = GeneratePseudoLegalMoves(board, onlyAttack: true);
                    board.Turn = board.Turn == Color.White ? Color.Black : Color.White;

                    // Check if opponent can attack square (3, 7) or (2, 7).
                    if (!enemyMoves.Exists(move => (move.To.File == 4 || move.To.File == 3 || move.To.File == 2) && move.To.Rank == 7)) {
                        int from = Move.Index(4, 7);
                        int to = Move.Index(2, 7);
                        moves.Add(new(from, to, Move.QueenCastleFlag) {
                            From = new(4, 7),
                            To = new(2, 7),
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
            if (m.IsCapture && m.CapturedPiece.Value.Type == PieceType.King) {
                isLegalMove = false;
                break;
            }
        }

        board.UndoMove(move, state);
        return isLegalMove;
    }

    private static void AddPawnPromotionMoves(List<Move> moves, Move move) {
        moves.Add(new(move.FromSquare, move.ToSquare, Move.PromotionFlag | Move.KnightPromotionFlag | move.Flags) {
            From = move.From,
            To = move.To,
            CapturedPiece = move.CapturedPiece
        });

        moves.Add(new(move.FromSquare, move.ToSquare, Move.PromotionFlag | Move.BishopPromotionFlag | move.Flags) {
            From = move.From,
            To = move.To,
            CapturedPiece = move.CapturedPiece
        });

        moves.Add(new(move.FromSquare, move.ToSquare, Move.PromotionFlag | Move.RookPromotionFlag | move.Flags) {
            From = move.From,
            To = move.To,
            CapturedPiece = move.CapturedPiece
        });

        moves.Add(new(move.FromSquare, move.ToSquare, Move.PromotionFlag | Move.QueenPromotionFlag | move.Flags) {
            From = move.From,
            To = move.To,
            CapturedPiece = move.CapturedPiece
        });
    }
}
