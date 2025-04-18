using Promotinator.Engine;
using Xunit.Abstractions;

namespace Promotinator.Tests;

// https://www.chessprogramming.org/Perft_Results
public class PerftTest {
    private readonly ITestOutputHelper _output;

    public PerftTest(ITestOutputHelper output) {
        _output = output;
    }

    [Theory]
    [InlineData([1, 20])]
    [InlineData([2, 400])]
    [InlineData([3, 8_902])]
    [InlineData([4, 197_281])]
    [InlineData([5, 4_865_609])]
    public void InitialPosition(int depth, int expected) {
        string initial = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        TestPosition(depth, expected, initial);
    }

    [Theory]
    [InlineData(1, 48)]
    [InlineData(2, 2_039)]
    [InlineData(3, 97_862)]
    [InlineData(4, 4_085_603)]
    public void Position2(int depth, int expected) {
        string position2 = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq -";
        TestPosition(depth, expected, position2);
    }

    [Theory]
    [InlineData(1, 14)]
    [InlineData(2, 191)]
    [InlineData(3, 2_812)]
    [InlineData(4, 43_238)]
    [InlineData(5, 674_624)]
    public void Position3(int depth, int expected) {
        string position3 = "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1";
        TestPosition(depth, expected, position3);
    }

    [Theory]
    [InlineData(1, 6)]
    [InlineData(2, 264)]
    [InlineData(3, 9_467)]
    [InlineData(4, 422_333)]
    public void Position4(int depth, int expected) {
        string position4 = "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1";
        TestPosition(depth, expected, position4);
    }

    [Theory]
    [InlineData(1, 44)]
    [InlineData(2, 1_486)]
    [InlineData(3, 62_379)]
    [InlineData(4, 2_103_487)]
    public void Position5(int depth, int expected) {
        string position5 = "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8";
        TestPosition(depth, expected, position5);
    }

    [Theory]
    [InlineData(1, 46)]
    [InlineData(2, 2_079)]
    [InlineData(3, 89_890)]
    [InlineData(4, 3_894_594)]
    public void Position6(int depth, int expected) {
        string position6 = "r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10";
        TestPosition(depth, expected, position6);
    }

    [Theory]
    [InlineData(1, 26)]
    [InlineData(2, 568)]
    [InlineData(3, 13_744)]
    [InlineData(4, 314_346)]
    [InlineData(5, 7_594_526)]
    public void KingAndRooks1(int depth, int expected) {
        string position = "r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1";
        TestPosition(depth, expected, position);
    }

    [Theory]
    [InlineData(1, 3)]
    [InlineData(2, 32)]
    [InlineData(3, 134)]
    [InlineData(4, 2_073)]
    [InlineData(5, 10_485)]
    public void KingAndRooks2(int depth, int expected) {
        string position = "8/8/8/8/8/8/6k1/4K2R b K - 0 1";
        TestPosition(depth, expected, position);
    }

    [Theory]
    [InlineData(1, 24)]
    [InlineData(2, 496)]
    [InlineData(3, 9_483)]
    [InlineData(4, 182_838)]
    [InlineData(5, 3_605_103)]
    public void KingAndKnights1(int depth, int expected) {
        string position = "n1n5/PPPk4/8/8/8/8/4Kppp/5N1N w - - 0 1";
        TestPosition(depth, expected, position);
    }

    [Theory]
    [InlineData(1, 7)]
    [InlineData(2, 49)]
    [InlineData(3, 378)]
    [InlineData(4, 2_902)]
    [InlineData(5, 24_122)]
    [InlineData(6, 199_002)]
    public void KingAndPawns1(int depth, int expected) {
        string position = "3k4/3pp3/8/8/8/8/3PP3/3K4 w - - 0 1";
        TestPosition(depth, expected, position);
    }

//    [Fact]
//    public void Debug() {
//        TestPosition(2, 2, "r3k2r/p1ppq1b1/bn2pnp1/4N3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1");
//    }

    private void TestPosition(int depth, int expected, string fen) {
        Board board = new(fen);
        int moves = CountMoves(board, depth);
        Assert.Equal(expected, moves);
    }

    private int CountMoves(Board board, int depth, bool debug = true) {
        if (depth == 0) {
            return 1;
        }

        int numMoves = 0;
        List<Move> moves = MoveGenerator.GenerateMoves(board);
        Console.WriteLine($"CountMoves depth:{depth}");
        foreach (Move move in moves) {
            Coord? lastEnPassantSquare = board.EnPassantSquare;
            CastlingRights lastCastlingRights = board.CastlingRights;

            board.MakeMove(move);

            int count = CountMoves(board, depth - 1, false);
            numMoves += count;

            if (debug) {
                _output.WriteLine($"{move}: {count}");
            }

            board.UndoMove(move, lastEnPassantSquare, lastCastlingRights);
        }

        return numMoves;
    }
}
